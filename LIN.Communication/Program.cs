using Http.Extensions;
using LIN.Access.Auth;
using LIN.Communication.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Create services to the container.
builder.Services.AddSignalR();
builder.Services.AddLINHttp();
builder.Services.AddLocalServices();

// Persistencia.
builder.Services.AddPersistence(builder.Configuration);

// App.
var app = builder.Build();

app.UseLINHttp();
app.UsePersistence();
app.UseAuthentication(builder.Configuration["services:auth"]);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Jwt.Open();

app.MapHub<LIN.Communication.Hubs.ChatHub>("/chat", options =>
{
    options.AllowStatefulReconnects = true;
    options.ApplicationMaxBufferSize = long.MaxValue;
});

app.Run();