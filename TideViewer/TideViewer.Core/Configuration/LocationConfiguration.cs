namespace TideViewer.Configuration;

/// <summary>
/// Configuration for geographic location
/// </summary>
public class LocationConfiguration
{
    /// <summary>
    /// Display name of the location
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Latitude in decimal degrees (-90 to 90)
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees (-180 to 180)
    /// </summary>
    public double Longitude { get; set; }
}
