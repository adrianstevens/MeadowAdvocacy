namespace TideViewer.Configuration;

/// <summary>
/// Configuration for API endpoint URLs
/// </summary>
public class ApiEndpointConfiguration
{
    /// <summary>
    /// Base URL for NOAA Tides and Currents API
    /// </summary>
    public string NoaaBaseUrl { get; set; } = "https://api.tidesandcurrents.noaa.gov/api/prod/datagetter";

    /// <summary>
    /// Base URL for Stormglass.io API
    /// </summary>
    public string StormglassBaseUrl { get; set; } = "https://api.stormglass.io";
}
