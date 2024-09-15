using LIN.Communication.Persistence.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LIN.Communication.Persistence.Extensions;

public static class PersistenceExtensions
{

    /// <summary>
    /// Agregar persistencia.
    /// </summary>
    /// <param name="services">Servicios.</param>
    /// <param name="configuration">Configuración.</param>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfigurationManager configuration)
    {

        services.AddDbContextPool<Context>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("release"));
        });

        services.AddScoped<Members, Members>();
        services.AddScoped<Conversations, Conversations>();
        services.AddScoped<Messages, Messages>();
        services.AddScoped<Profiles, Profiles>();
        return services;

    }


    /// <summary>
    /// Utilizar persistencia.  
    /// </summary>
    public static IApplicationBuilder UsePersistence(this IApplicationBuilder app)
    {
        try
        {
            var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetService<Context>();
            context?.Database.EnsureCreated();
        }
        catch (Exception)
        {
        }
        return app;
    }

}