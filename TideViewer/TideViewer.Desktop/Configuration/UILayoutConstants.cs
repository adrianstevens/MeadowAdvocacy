namespace TideViewer.Desktop.Configuration;

/// <summary>
/// Constants for UI layout positions and dimensions
/// </summary>
public static class UILayoutConstants
{
    public const int Column1X = 40;
    public const int Column2X = 180;

    public static class TideGraph
    {
        public const int X = 274;
        public const int Y = 200;
        public const int Width = 322;
        public const int Height = 245;
        public const int YAxisGridCount = 3;
        public const int XAxisIntervalHours = 6;
    }

    public static class WeatherDisplay
    {
        public const int TemperatureX = 150;
        public const int TemperatureY = 20;
        public const int IconX = 20;
        public const int IconY = 20;
    }

    public static class GraphPadding
    {
        public const double MinValuePadding = 2.0;
        public const double MaxValuePadding = 2.0;
    }
}
