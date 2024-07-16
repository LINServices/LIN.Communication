using LIN.Communication.Services.Interfaces;

namespace LIN.Communication.Services;


public static class Extensions
{

    /// <summary>
    /// Add local services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageSender, MessageSender>();
        return services;
    }


}