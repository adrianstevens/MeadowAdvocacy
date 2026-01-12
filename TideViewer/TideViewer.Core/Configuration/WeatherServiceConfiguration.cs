namespace TideViewer.Configuration;

/// <summary>
/// Configuration for weather service API
/// </summary>
public class WeatherServiceConfiguration
{
    /// <summary>
    /// API key for OpenWeatherMap service
    /// </summary>
    public string OpenWeatherMapApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for OpenWeatherMap API
    /// </summary>
    public string OpenWeatherMapBaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";

    /// <summary>
    /// Temperature units (metric or imperial)
    /// </summary>
    public string Units { get; set; } = "metric";
}
