namespace LIN.Communication.Services;


public class Configuration
{


    private static IConfiguration? Config;

    private static readonly bool IsStart = false;


    public static string GetConfiguration(string route)
    {

        if (!IsStart || Config == null)
        {
            var configBuilder = new ConfigurationBuilder()
                     .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Config = configBuilder.Build();
        }

        return Config[route] ?? string.Empty;

    }

}
