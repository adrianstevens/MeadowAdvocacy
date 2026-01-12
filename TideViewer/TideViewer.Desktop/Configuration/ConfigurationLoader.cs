using Microsoft.Extensions.Configuration;

namespace TideViewer.Desktop.Configuration;

/// <summary>
/// Loads configuration from appsettings.json and environment variables
/// </summary>
public static class ConfigurationLoader
{
    /// <summary>
    /// Loads configuration from JSON files and environment variables
    /// </summary>
    /// <returns>Configuration root</returns>
    public static IConfiguration LoadConfiguration()
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("TIDEVIEWER_")
            .Build();
    }
}
