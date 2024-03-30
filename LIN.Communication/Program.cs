using LIN.Communication.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string sqlConnection = builder.Configuration["ConnectionStrings:release"] ?? string.Empty;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

Conexión.SetStringConnection(sqlConnection);


if (sqlConnection.Length > 0)
{
    // SQL Server
    builder.Services.AddDbContext<Context>(options =>
    {
        options.UseSqlServer(sqlConnection);
    });
}


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAnyOrigin");

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
