using Http.Extensions;
using LIN.Access.Auth;
using LIN.Communication.Hangfire;
using LIN.Communication.Persistence;
using LIN.Communication.Persistence.Extensions;

try
{




    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    return true;
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // Create services to the container.
    builder.Services.AddSignalR();
    builder.Services.AddLINHttp(useCors: false);
    builder.Services.AddLocalServices();
    builder.Services.AddAuthenticationService(builder.Configuration["services:auth"], builder.Configuration["policy:linapp"]);

    // Persistencia.
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddSettingsHangfire(builder.Configuration);

    // App.
    var app = builder.Build();

    app.UseCors("AllowAll"); // aplica la política a todo

    app.UseLINHttp(useGateway: true);
    app.UsePersistence();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.UseSettingsHangfire();

    Jwt.Open(builder.Configuration["jwt:key"]);

    app.MapHub<LIN.Communication.Hubs.ChatHub>("/chat", options =>
    {
        options.AllowStatefulReconnects = true;
        options.ApplicationMaxBufferSize = long.MaxValue;
    });

    app.MapHub<LIN.Communication.Hubs.CallHub>("/hub/calls", options =>
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
}
catch (Exception ex)
{
    if (!File.Exists("wwwroot/error.txt"))
    {
        File.Create("wwwroot/error.txt");
    }

    File.WriteAllText("wwwroot/error.txt", ex.Message + ex.StackTrace);
}