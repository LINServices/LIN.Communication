using Http.Extensions;
using LIN.Communication.Data;
using LIN.Communication.Services.Iam;

var builder = WebApplication.CreateBuilder(args);

// Create services to the container.
builder.Services.AddSignalR();

builder.Services.AddLINHttp();

string sqlConnection = builder.Configuration["ConnectionStrings:release"] ?? string.Empty;

Conexión.SetStringConnection(sqlConnection);

if (sqlConnection.Length > 0)
{
    // SQL Server
    builder.Services.AddDbContext<Context>(options =>
    {
        options.UseSqlServer(sqlConnection);
    });
}

builder.Services.AddSingleton<IIamService, Conversation>();

var app = builder.Build();

app.UseLINHttp();

try
{
    // Si la base de datos no existe
    using var scope = app.Services.CreateScope();
    var dataContext = scope.ServiceProvider.GetRequiredService<Context>();
    var res = dataContext.Database.EnsureCreated();
}
catch
{ }

Jwt.Open();

LIN.Access.Auth.Build.Init();

app.UseHttpsRedirection();

app.MapHub<LIN.Communication.Hubs.ChatHub>("/chat", options =>
{
    options.AllowStatefulReconnects = true;
    options.ApplicationMaxBufferSize = long.MaxValue;
});

app.UseAuthorization();
app.MapControllers();

app.Run();
