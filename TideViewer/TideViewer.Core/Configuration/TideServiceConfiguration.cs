namespace TideViewer.Configuration;

/// <summary>
/// Configuration for tide service API connections
/// </summary>
public class TideServiceConfiguration
{
    /// <summary>
    /// API key for Stormglass.io service
    /// </summary>
    public string StormglassApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Application name to identify requests to NOAA API
    /// </summary>
    public string NoaaApplicationName { get; set; } = "MeadowTideGraph";

    /// <summary>
    /// Default datum for NOAA tide predictions (e.g., "MLLW" - Mean Lower Low Water)
    /// </summary>
    public string NoaaDefaultDatum { get; set; } = "MLLW";

    /// <summary>
    /// Interval in minutes for NOAA tide prediction resolution
    /// </summary>
    public int NoaaIntervalMinutes { get; set; } = 6;
}
