using System;

namespace TideViewer.Models;

/// <summary>
/// Daily weather forecast data
/// </summary>
public class DailyForecast
{
    public DateTime Date { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
}
