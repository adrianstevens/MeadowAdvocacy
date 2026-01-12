using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using TideViewer;
using TideViewer.assets;
using TideViewer.Configuration;
using TideViewer.Desktop.Configuration;

namespace SilkDisplay_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;
    static StormglassTideService tideService;

    static IFont fontLarge;
    static IFont fontMedium;
    static IFont fontSmall;

    static IPixelBuffer ditheredWeatherToday;

    static Program()
    {
    }

    public static void Main()
    {
        Initialize();
        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        // Load configuration
        var config = ConfigurationLoader.LoadConfiguration();
        var tideServiceConfig = config.GetSection("TideService").Get<TideServiceConfiguration>();
        var apiEndpointsConfig = config.GetSection("ApiEndpoints").Get<ApiEndpointConfiguration>();

        // Validate configuration (warning only - app works with sample data)
        if (tideServiceConfig == null || string.IsNullOrWhiteSpace(tideServiceConfig.StormglassApiKey))
        {
            Console.WriteLine("WARNING: StormglassApiKey not configured. Using sample data.");
            Console.WriteLine("To use real API data, set it in appsettings.json or environment variable: TIDEVIEWER_TideService__StormglassApiKey");
            tideServiceConfig = new TideServiceConfiguration { StormglassApiKey = "dummy-key-for-development" };
        }

        fontLarge = new Font16x24();
        fontMedium = new Font12x20();
        fontSmall = new Font8x12();

        display = new SilkDisplay(displayScale: 1f);

        var virtualDisplay = new SimulatedEpd5in65f(rotate: true, displayRenderer: display);

        ditheredWeatherToday = Resources.GetDitheredIcon(IconType.night_clear);

        graphics = new MicroGraphics(virtualDisplay)
        {
            CurrentFont = fontMedium,
            Stroke = 1,
        };

        // Create service with configuration
        string baseUrl = apiEndpointsConfig?.StormglassBaseUrl ?? "https://api.stormglass.io";
        tideService = new StormglassTideService(tideServiceConfig.StormglassApiKey, baseUrl);
    }

    public static void Run()
    {
        // Load configuration
        var config = ConfigurationLoader.LoadConfiguration();
        var locationConfig = config.GetSection("Location").Get<LocationConfiguration>();
        var uiConfig = config.GetSection("UI").Get<UIConfiguration>();

        int xCol1 = uiConfig?.ColumnPositions?.Col1 ?? UILayoutConstants.Column1X;
        int xCol2 = uiConfig?.ColumnPositions?.Col2 ?? UILayoutConstants.Column2X;

        double lat = locationConfig?.Latitude ?? 49.208979;
        double lng = locationConfig?.Longitude ?? -123.809392;

        // Time window: now → next 24h
        var start = DateTime.Now;
        var end = start.AddHours(24);

        // Using sample data for development/testing
        // For production with API key configured, you would call:
        // var points = await tideService.GetSeaLevelAsync(lat, lng, start, end);
        var points = GetSampleTideData();

        var sunrise = Resources.GetDitheredIcon(IconType.sunrise_sm);
        var wind = Resources.GetDitheredIcon(IconType.windy_sm);
        var uv = Resources.GetDitheredIcon(IconType.sunny_sm);
        var humidity = Resources.GetDitheredIcon(IconType.humidity_sm);
        var pressure = Resources.GetDitheredIcon(IconType.pressure_sm);
        var aqi = Resources.GetDitheredIcon(IconType.aqi_sm);


        _ = Task.Run(() =>
        {
            while (true)
            {
                graphics.Clear(Color.White);

                graphics.DrawBuffer(20, 20, ditheredWeatherToday);


                graphics.DrawText(150, 20, "22", Color.Black, ScaleFactor.X2, font: fontLarge);
                graphics.DrawText(220, 20, "°C", Color.Black, ScaleFactor.X1, font: fontMedium);
                graphics.DrawText(150, 70, "Feels like 80°", Color.Black, ScaleFactor.X1, font: fontSmall);

                graphics.DrawText(display!.Width - 4, 4, "Gabriola,BC", Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontLarge);
                graphics.DrawText(display!.Width - 4, 30, "Sunday,September 7", Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontMedium);

                graphics.DrawBuffer(4, 200, sunrise);
                graphics.DrawText(xCol1, 200, "Sunrise", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 212, "7:17am", Color.Black, font: fontMedium);

                graphics.DrawBuffer(144, 200, sunrise);
                graphics.DrawText(xCol2, 200, "Sunset", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 212, "7:17pm", Color.Black, font: fontMedium);

                graphics.DrawBuffer(4, 250, wind);
                graphics.DrawText(xCol1, 250, "Wind", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 262, "12kn", Color.Black, font: fontMedium);

                graphics.DrawBuffer(144, 250, uv);
                graphics.DrawText(xCol2, 250, "UV Index", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 262, "12", Color.Black, font: fontMedium);

                graphics.DrawBuffer(4, 300, humidity);
                graphics.DrawText(xCol1, 300, "Humidity", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 312, "80%", Color.Black, font: fontMedium);

                graphics.DrawBuffer(144, 300, pressure);
                graphics.DrawText(xCol2, 300, "Pressure", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 312, "1.01atm", Color.Black, font: fontMedium);

                graphics.DrawBuffer(4, 350, aqi);
                graphics.DrawText(xCol1, 350, "Air Quality", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 362, "30", Color.Black, font: fontMedium);

                graphics.DrawRectangle(144, 350, 30, 30, Color.Black, false);
                graphics.DrawText(xCol2, 350, "Visibility", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 362, "> 3km", Color.Black, font: fontMedium);

                var graphSettings = uiConfig?.TideGraph;
                int graphX = graphSettings?.X ?? UILayoutConstants.TideGraph.X;
                int graphY = graphSettings?.Y ?? UILayoutConstants.TideGraph.Y;
                int graphW = graphSettings?.Width ?? UILayoutConstants.TideGraph.Width;
                int graphH = graphSettings?.Height ?? UILayoutConstants.TideGraph.Height;

                DrawTideGraph(points, graphX, graphY, graphX + graphW, graphY + graphH, "m");

                graphics.Show();

            }
        });

        display!.Run();
    }

    static void DrawTideGraph(IList<TidePoint> points, int x0, int y0, int x1, int y1, string yUnits = "m")
    {
        // normalize corners → box
        int left = Math.Min(x0, x1);
        int top = Math.Min(y0, y1);
        int right = Math.Max(x0, x1);
        int bottom = Math.Max(y0, y1);

        int W = right - left;
        int H = bottom - top;

        // frame (outer box)
        graphics.DrawHorizontalGradient(left, top, W, H, Color.White, Color.SkyBlue);

        // no data?
        if (points == null || points.Count < 2)
        {
            graphics.CurrentFont = new Font6x8();
            graphics.DrawText(left + 4, top + 4, "no tide data");
            return;
        }

        // scale
        double minV = points.Min(p => p.Level) - UILayoutConstants.GraphPadding.MinValuePadding;
        double maxV = points.Max(p => p.Level) + UILayoutConstants.GraphPadding.MaxValuePadding;

        if (Math.Abs(maxV - minV) < 0.1)
        {
            maxV += 0.5;
            minV -= 0.5;
        }

        DateTime timeStart = points.First().Time;
        DateTime timeEnd = points.Last().Time;
        double totalMinutes = Math.Max(1, (timeEnd - timeStart).TotalMinutes);

        // y grid + labels
        graphics.CurrentFont = new Font6x8();
        int yGrids = UILayoutConstants.TideGraph.YAxisGridCount;

        for (int i = 0; i <= yGrids; i++)
        {
            int y = top + (int)Math.Round(H * (1.0 - i / (double)yGrids));
            graphics.DrawLine(left, y, left + W, y, Color.Black);

            double v = minV + (maxV - minV) * (i / (double)yGrids);
            graphics.DrawText(left + 2, y - 9, v.ToString("0.0", CultureInfo.InvariantCulture) + yUnits, Color.Black);
        }

        // x ticks
        var step = TimeSpan.FromHours(UILayoutConstants.TideGraph.XAxisIntervalHours);
        var tick = RoundUp(timeStart, step);

        for (var t = tick; t <= timeEnd; t = t.Add(step))
        {
            int x = left + (int)Math.Round(W * ((t - timeStart).TotalMinutes / totalMinutes));
            graphics.DrawLine(x, top, x, top + H, Color.Black);
        }

        // plot line
        int? lx = null, ly = null;

        foreach (var p in points)
        {
            int x = left + (int)Math.Round(W * ((p.Time - timeStart).TotalMinutes / totalMinutes));
            int y = top + (int)Math.Round(H * (1.0 - (p.Level - minV) / (maxV - minV)));

            if (lx.HasValue)
            {
                graphics.DrawLine(lx.Value, ly!.Value, x, y, Color.Blue);
            }

            lx = x; ly = y;
        }
    }

    static DateTime RoundUp(DateTime dt, TimeSpan step)
    {
        long ticks = ((dt.Ticks + step.Ticks - 1) / step.Ticks) * step.Ticks;
        return new DateTime(ticks, dt.Kind);
    }

    private static List<TidePoint> GetSampleTideData()
    {
        var baseTime = DateTime.Now.Date;
        return new List<TidePoint>
        {
            new TidePoint(baseTime.AddHours(0), 0.72),
            new TidePoint(baseTime.AddHours(1), 0.75),
            new TidePoint(baseTime.AddHours(2), 1.12),
            new TidePoint(baseTime.AddHours(3), 1.61),
            new TidePoint(baseTime.AddHours(4), 1.90),
            new TidePoint(baseTime.AddHours(5), 1.71),
            new TidePoint(baseTime.AddHours(6), 0.95),
            new TidePoint(baseTime.AddHours(7), -0.33),
            new TidePoint(baseTime.AddHours(8), -1.97),
            new TidePoint(baseTime.AddHours(9), -3.77),
            new TidePoint(baseTime.AddHours(10), -5.45),
            new TidePoint(baseTime.AddHours(11), -6.56),
            new TidePoint(baseTime.AddHours(12), -6.82),
            new TidePoint(baseTime.AddHours(13), -6.14),
            new TidePoint(baseTime.AddHours(14), -4.69),
            new TidePoint(baseTime.AddHours(15), -2.59),
            new TidePoint(baseTime.AddHours(16), -0.26),
            new TidePoint(baseTime.AddHours(17), 1.80),
            new TidePoint(baseTime.AddHours(18), 3.28),
            new TidePoint(baseTime.AddHours(19), 4.04),
            new TidePoint(baseTime.AddHours(20), 4.07),
            new TidePoint(baseTime.AddHours(21), 3.41),
            new TidePoint(baseTime.AddHours(22), 2.30),
            new TidePoint(baseTime.AddHours(23), 1.05),
            new TidePoint(baseTime.AddHours(24), 0.13),
        };
    }
}

//<!=SNOP=>