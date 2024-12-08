namespace LIN.Communication.Hangfire.Services;

public class FileCache
{

    /// <summary>
    /// Lista de cache.
    /// </summary>
    private static Dictionary<string, string> _cache = [];


    /// <summary>
    /// Obtener el contenido de un archivo.
    /// </summary>
    /// <param name="path">Path</param>
    public static string ReadContent(string path)
    {
        // No existe el archivo.
        if (!File.Exists(path))
            return "";

        // Si el archivo ya está en caché, devolverlo.
        _cache.TryGetValue(path, out string? content);

        // No existe el archivo.
        if (content is null)
        {
            content = File.ReadAllText(path);
            _cache.Add(path, content);
        }
        return content;
    }

}