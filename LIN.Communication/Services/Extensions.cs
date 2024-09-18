using LIN.Communication.Services.Iam;
using LIN.Communication.Services.Interfaces;

namespace LIN.Communication.Services;

public static class Extensions
{

    /// <summary>
    /// Add local services.
    /// </summary>
    public static IServiceCollection AddLocalServices(this IServiceCollection services)
    {
        services.AddScoped<IIamService, Conversation>();
        services.AddScoped<IIAService, IAService>();
        services.AddScoped<IMessageSender, MessageSender>();
        return services;
    }

}