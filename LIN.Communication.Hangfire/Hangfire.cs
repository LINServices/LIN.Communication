using Hangfire;
using Hangfire.PostgreSql;
using LIN.Communication.Hangfire.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LIN.Communication.Hangfire;

public static class Hangfire
{

    /// <summary>
    /// Agregar servicios de Hangfire.
    /// </summary>
    /// <param name="services">Servicios.</param>
    /// <param name="manager">Manager.</param>
    public static IServiceCollection AddSettingsHangfire(this IServiceCollection services, IConfigurationManager manager)
    {

        // Add Hangfire services.
        services.AddHangfire(config =>
        {
            config.UsePostgreSqlStorage(manager.GetConnectionString("hangfire") ?? string.Empty);
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
            config.UseSimpleAssemblyNameTypeSerializer();
            config.UseRecommendedSerializerSettings();
        });

        services.AddHangfireServer(options =>
        {
            options.Queues = ["default"];
        });
        JwtService.Open();

        // Jobs.
        return services;
    }


    /// <summary>
    /// Usar servicios de Hangfire.
    /// </summary>
    /// <param name="app">App.</param>
    public static IApplicationBuilder UseSettingsHangfire(this IApplicationBuilder app)
    {
        // Configuración del tablero.
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AsyncAuthorization = [new IdentityAuthorization()],
            DarkModeEnabled = true,
            DashboardTitle = "LIN Communication"
        });

        return app;
    }

}