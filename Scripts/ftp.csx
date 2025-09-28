#r "nuget: FluentFTP, 53.0.1"
#nullable enable
using System;
using System.IO;
using System.Net;
using System.Threading;          // <-- Necesario p/ CancellationToken, CancellationTokenSource
using System.Threading.Tasks;    // <-- Necesario p/ Task, async/await
using FluentFTP;

var options = new FtpCleanerOptions
{
    Host = Environment.GetEnvironmentVariable("FTP_HOST") ?? "",
    Port = 21,
    User = Environment.GetEnvironmentVariable("FTP_USER") ?? "",
    Pass = Environment.GetEnvironmentVariable("FTP_PASS") ?? "",
    RemoteDir = Environment.GetEnvironmentVariable("FTP_DIR") ?? "",
    UseFtps = false,             // true si tu servidor requiere FTPS (TLS explícito)
    MaxTries = 12,
    SleepBetweenTries = TimeSpan.FromSeconds(6)
};

var progress = new Progress<string>(msg => Console.WriteLine(msg));
var cts = new CancellationTokenSource();

try
{
    await FtpCleaner.CleanAsync(options, progress, cts.Token);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"❌ Error: {ex.Message}");
    Environment.ExitCode = 1;
}


public sealed class FtpCleanerOptions
{
    public string Host { get; init; } = default!;
    public int Port { get; init; } = 21;
    public string User { get; init; } = default!;
    public string Pass { get; init; } = default!;
    public string RemoteDir { get; init; } = "/"; // raíz en el FTP donde limpiar
    public bool UseFtps { get; init; } = false;   // true = TLS explícito (FTPS)
    public int MaxTries { get; init; } = 12;
    public TimeSpan SleepBetweenTries { get; init; } = TimeSpan.FromSeconds(6);
    public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(15);
}

public static class FtpCleaner
{
    public static async Task CleanAsync(FtpCleanerOptions opt, IProgress<string>? log = null, CancellationToken ct = default)
    {
        using var client = new FtpClient(opt.Host, opt.Port)
        {
         Credentials = new(opt.User, opt.Pass)    
        };

         client.Connect();

        // Normaliza RemoteDir (sin barra final, excepto si es "/")
        var baseDir = NormalizeDir(opt.RemoteDir);

        for (int attempt = 1; attempt <= opt.MaxTries; attempt++)
        {
            log?.Report($"Intento {attempt}/{opt.MaxTries}: limpieza recursiva en {baseDir} (solo se conserva 'appsettings.json')...");

            // 1) Listado recursivo (directorios y archivos, con rutas absolutas dentro del FTP)
            var allItems = await SafeGetListingRecursive(client, baseDir, ct);

            // 2) Separa archivos y directorios
            var allFiles = allItems.Where(i => i.Type == FtpObjectType.File).ToList();
            var allDirs = allItems.Where(i => i.Type == FtpObjectType.Directory).ToList();

            // 3) Archivos a conservar: EXACTAMENTE appsettings.json (case-insensitive), en cualquier subcarpeta
            var keptFiles = allFiles
                .Where(f => f.Name.StartsWith("appsettings.", StringComparison.OrdinalIgnoreCase))
                .Select(f => NormalizePath(f.FullName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 4) Directorios protegidos: el contenedor y todos los ancestros de cada appsettings.json
            var protectedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kf in keptFiles)
            {
                foreach (var anc in AncestorDirsOfFile(kf, baseDir))
                {
                    protectedDirs.Add(anc);
                }
            }
            // Protege también la base (por consistencia)
            protectedDirs.Add(baseDir);

            // 5) Archivos a borrar: todos los demás
            var filesToDelete = allFiles
                .Select(f => NormalizePath(f.FullName))
                .Where(full => !keptFiles.Contains(full))
                .ToList();

            // 6) Borra archivos primero
            if (filesToDelete.Count > 0)
            {
                log?.Report($"Borrando archivos ({filesToDelete.Count})...");
                foreach (var file in filesToDelete)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        log?.Report($"  rm: {file}");
                        client.DeleteFile(file);
                    }
                    catch (Exception ex)
                    {
                        log?.Report($"  ⚠️ no se pudo borrar {file}: {ex.Message}");
                    }
                }
            }

            // 7) Directorios borrables: los NO protegidos
            var dirsToDelete = allDirs
                .Select(d => NormalizeDir(d.FullName))
                .Where(d => !protectedDirs.Contains(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                // de lo más profundo a lo menos (por cantidad de '/')
                .OrderByDescending(d => Depth(d))
                .ToList();

            // 8) Borra directorios **solo si están vacíos**
            if (dirsToDelete.Count > 0)
            {
                log?.Report($"Borrando directorios vacíos ({dirsToDelete.Count})...");
                foreach (var dir in dirsToDelete)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        // Si no existe, sigue
                        if (!client.DirectoryExists(dir)) continue;

                        // Comprueba si está vacío
                        var children = client.GetListing(dir, FtpListOption.Recursive);
                        if (children == null || children.Length == 0)
                        {
                            log?.Report($"  rmdir: {dir}/");
                            client.DeleteDirectory(dir); // seguro si está vacío
                        }
                    }
                    catch (Exception ex)
                    {
                        log?.Report($"  ⚠️ no se pudo borrar {dir}: {ex.Message}");
                    }
                }
            }

            // 9) Recuento: si no queda nada por borrar (archivos no-kept + dirs no protegidos vacíos), terminamos
            var after = await SafeGetListingRecursive(client, baseDir, ct);
            var afterFiles = after.Where(i => i.Type == FtpObjectType.File).Select(i => NormalizePath(i.FullName)).ToList();
            var afterDirs = after.Where(i => i.Type == FtpObjectType.Directory).Select(i => NormalizeDir(i.FullName)).ToList();

            var remainingFiles = afterFiles.Count(f => !keptFiles.Contains(f));
            var remainingDirs = afterDirs.Count(d => !protectedDirs.Contains(d)); // algunos pueden seguir no vacíos

            var remaining = remainingFiles + remainingDirs;
            log?.Report($"Quedan {remaining} elementos borrables (excluyendo appsettings.json).");

            if (remaining == 0)
            {
                log?.Report("Directorio remoto limpio ✅ (se conservaron únicamente archivos 'appsettings.json').");
                break;
            }

            if (attempt == opt.MaxTries)
            {
                throw new InvalidOperationException($"No se pudo limpiar el remoto tras {opt.MaxTries} intentos.");
            }

            await Task.Delay(opt.SleepBetweenTries, ct);
        }

        client.Disconnect();
    }

    // Helpers

    private static async Task<FtpListItem[]> SafeGetListingRecursive(FtpClient client, string dir, CancellationToken ct)
    {
        // Usa Recursive para traer todo de una pasada; si el servidor no soporta, puedes
        // cambiar a un recorrido manual (BFS/DFS) con GetListing por nivel.
        return client.GetListing(dir, FtpListOption.Recursive);
    }

    private static string NormalizePath(string p)
    {
        if (string.IsNullOrWhiteSpace(p)) return "/";
        p = p.Replace('\\', '/');
        // quita doble slash (excepto si comienza con ftp raíz //, no aplica aquí)
        while (p.Contains("//")) p = p.Replace("//", "/");
        return p;
    }

    private static string NormalizeDir(string p)
    {
        p = NormalizePath(p);
        if (p.Length > 1 && p.EndsWith("/", StringComparison.Ordinal)) p = p.TrimEnd('/');
        return p == "" ? "/" : p;
    }

    private static int Depth(string path)
    {
        var p = NormalizePath(path);
        if (p == "/") return 0;
        return p.Count(c => c == '/');
    }

    private static IEnumerable<string> AncestorDirsOfFile(string fileFullPath, string baseDir)
    {
        // fileFullPath: /a/b/appsettings.json
        // retorna: /a/b, /a, / (hasta baseDir)
        baseDir = NormalizeDir(baseDir);
        var dir = NormalizeDir(System.IO.Path.GetDirectoryName(fileFullPath)?.Replace('\\', '/') ?? "/");
        if (dir == "/")
        {
            yield return "/";
            yield break;
        }

        while (true)
        {
            yield return dir;
            if (dir.Equals(baseDir, StringComparison.OrdinalIgnoreCase) || dir == "/")
                yield break;
            var lastSlash = dir.LastIndexOf('/');
            if (lastSlash <= 0)
            {
                dir = "/";
            }
            else
            {
                dir = dir[..lastSlash];
                if (dir.Length == 0) dir = "/";
            }
        }
    }
}
