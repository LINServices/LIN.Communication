using Http.Extensions;
using LIN.Access.Auth;
using LIN.Communication.Hangfire;
using LIN.Communication.Persistence;
using LIN.Communication.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Create services to the container.
builder.Services.AddSignalR();
builder.Services.AddLINHttp();
builder.Services.AddLocalServices();
builder.Services.AddAuthenticationService(builder.Configuration["services:auth"], builder.Configuration["policy:linapp"]);

// Persistencia.
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSettingsHangfire(builder.Configuration);

// App.
var app = builder.Build();

app.UseLINHttp();
app.UsePersistence();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseSettingsHangfire();

Jwt.Open();

app.MapHub<LIN.Communication.Hubs.ChatHub>("/chat", options =>
{
    options.AllowStatefulReconnects = true;
    options.ApplicationMaxBufferSize = long.MaxValue;
});

builder.Services.AddDatabaseAction(() =>
{
    var context = app.Services.GetRequiredService<Context>();
    context.Profiles.Where(x => x.Id == 0).FirstOrDefaultAsync();
    return "Success";
});

app.Run();