using System;

namespace TideViewer.Models;

/// <summary>
/// Current weather conditions
/// </summary>
public class WeatherData
{
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public int Humidity { get; set; }
    public double Pressure { get; set; }
    public double WindSpeed { get; set; }
    public int CloudCover { get; set; }
    public double Visibility { get; set; }
    public double UvIndex { get; set; }
    public int AirQualityIndex { get; set; }
    public int PrecipitationChance { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public DateTime Timestamp { get; set; }
    public MoonPhase MoonPhase { get; set; }
}
