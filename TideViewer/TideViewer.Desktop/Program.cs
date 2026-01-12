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
using TideViewer.Models;

namespace SilkDisplay_Sample;

//<!=SNIP=>

public class Program
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;
    static StormglassTideService tideService;
    static OpenWeatherMapService weatherService;

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
        var weatherServiceConfig = config.GetSection("WeatherService").Get<WeatherServiceConfiguration>();
        var apiEndpointsConfig = config.GetSection("ApiEndpoints").Get<ApiEndpointConfiguration>();

        // Validate tide service configuration (warning only - app works with sample data)
        if (tideServiceConfig == null || string.IsNullOrWhiteSpace(tideServiceConfig.StormglassApiKey))
        {
            Console.WriteLine("WARNING: StormglassApiKey not configured. Using sample tide data.");
            Console.WriteLine("To use real API data, set it in appsettings.json or environment variable: TIDEVIEWER_TideService__StormglassApiKey");
            tideServiceConfig = new TideServiceConfiguration { StormglassApiKey = "dummy-key-for-development" };
        }

        // Validate weather service configuration (warning only - app works with sample data)
        if (weatherServiceConfig == null || string.IsNullOrWhiteSpace(weatherServiceConfig.OpenWeatherMapApiKey))
        {
            Console.WriteLine("WARNING: OpenWeatherMapApiKey not configured. Using sample weather data.");
            Console.WriteLine("To use real weather data, set it in appsettings.json or environment variable: TIDEVIEWER_WeatherService__OpenWeatherMapApiKey");
            Console.WriteLine("Get a free API key from: https://openweathermap.org/api");
            weatherServiceConfig = new WeatherServiceConfiguration { OpenWeatherMapApiKey = "dummy-key-for-development" };
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

        // Create services with configuration
        string baseUrl = apiEndpointsConfig?.StormglassBaseUrl ?? "https://api.stormglass.io";
        tideService = new StormglassTideService(tideServiceConfig.StormglassApiKey, baseUrl);
        weatherService = new OpenWeatherMapService(weatherServiceConfig);
    }

    public static void Run()
    {
        // Load configuration
        var config = ConfigurationLoader.LoadConfiguration();
        var locationConfig = config.GetSection("Location").Get<LocationConfiguration>();
        var uiConfig = config.GetSection("UI").Get<UIConfiguration>();
        var weatherConfig = config.GetSection("WeatherService").Get<WeatherServiceConfiguration>();

        int xCol1 = uiConfig?.ColumnPositions?.Col1 ?? UILayoutConstants.Column1X;
        int xCol2 = uiConfig?.ColumnPositions?.Col2 ?? UILayoutConstants.Column2X;

        double lat = locationConfig?.Latitude ?? 49.208979;
        double lng = locationConfig?.Longitude ?? -123.809392;
        string locationName = locationConfig?.Name ?? "Unknown";

        // Time window: now → next 24h
        var start = DateTime.Now;
        var end = start.AddHours(24);

        // Fetch weather data (or use sample if API key not configured)
        WeatherData? weather = null;
        bool useRealWeather = weatherConfig != null && !string.IsNullOrWhiteSpace(weatherConfig.OpenWeatherMapApiKey)
                               && weatherConfig.OpenWeatherMapApiKey != "dummy-key-for-development";

        if (useRealWeather)
        {
            try
            {
                weather = Task.Run(async () =>
                {
                    var w = await weatherService.GetCurrentWeatherAsync(lat, lng);
                    w.UvIndex = await weatherService.GetUvIndexAsync(lat, lng);
                    w.MoonPhase = MoonPhaseCalculator.GetMoonPhase(DateTime.UtcNow);
                    return w;
                }).Result;
                Console.WriteLine("Successfully fetched weather data from OpenWeatherMap");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch weather data: {ex.Message}. Using sample data.");
                weather = null;
            }
        }

        // Fall back to sample weather data if not available
        if (weather == null)
        {
            weather = GetSampleWeatherData();
        }

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
        var night_clear = Resources.GetDitheredIcon(IconType.night_clear_sm);


        _ = Task.Run(() =>
        {
            while (true)
            {
                graphics.Clear(Color.White);

                graphics.DrawBuffer(20, 20, ditheredWeatherToday);

                // Temperature display
                string tempUnit = weatherConfig?.Units == "imperial" ? "°F" : "°C";
                graphics.DrawText(150, 24, $"{weather.Temperature:F0}", Color.Black, ScaleFactor.X2, font: fontLarge);
                graphics.DrawText(230, 30, tempUnit, Color.Black, ScaleFactor.X1, font: fontMedium);
                graphics.DrawText(150, 72, $"Feels like {weather.FeelsLike:F0}°", Color.Black, ScaleFactor.X1, font: fontSmall);

                // Location and date
                graphics.DrawText(display!.Width - 4, 4, locationName, Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontLarge);
                graphics.DrawText(display!.Width - 4, 30, DateTime.Now.ToString("dddd, MMMM d"), Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontMedium);

                // Sunrise
                graphics.DrawBuffer(4, 200, sunrise);
                graphics.DrawText(xCol1, 200, "Sunrise", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 212, weather.Sunrise.ToString("h:mmtt").ToLower(), Color.Black, font: fontMedium);

                // Sunset
                graphics.DrawBuffer(144, 200, sunrise);
                graphics.DrawText(xCol2, 200, "Sunset", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 212, weather.Sunset.ToString("h:mmtt").ToLower(), Color.Black, font: fontMedium);

                // Wind speed (convert m/s to knots if metric)
                graphics.DrawBuffer(4, 250, wind);
                graphics.DrawText(xCol1, 250, "Wind", Color.Black, font: fontSmall);
                double windKnots = weatherConfig?.Units == "metric" ? weather.WindSpeed * 1.944 : weather.WindSpeed;
                graphics.DrawText(xCol1, 262, $"{windKnots:F0}kn", Color.Black, font: fontMedium);

                // UV Index
                graphics.DrawBuffer(144, 250, uv);
                graphics.DrawText(xCol2, 250, "UV Index", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 262, $"{weather.UvIndex:F0}", Color.Black, font: fontMedium);

                // Humidity
                graphics.DrawBuffer(4, 300, humidity);
                graphics.DrawText(xCol1, 300, "Humidity", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 312, $"{weather.Humidity}%", Color.Black, font: fontMedium);

                // Pressure
                graphics.DrawBuffer(144, 300, pressure);
                graphics.DrawText(xCol2, 300, "Pressure", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 312, $"{weather.Pressure:F2}atm", Color.Black, font: fontMedium);

                // Cloud cover (using AQI icon as placeholder)
                graphics.DrawBuffer(4, 350, aqi);
                graphics.DrawText(xCol1, 350, "Clouds", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 362, $"{weather.CloudCover}%", Color.Black, font: fontMedium);

                // Visibility
                graphics.DrawRectangle(144, 350, 30, 30, Color.Black, false);
                graphics.DrawText(xCol2, 350, "Visibility", Color.Black, font: fontSmall);
                graphics.DrawText(xCol2, 362, $"{weather.Visibility:F1}km", Color.Black, font: fontMedium);

                // Moon Phase (display below the weather metrics)
                graphics.DrawBuffer(4, 400, night_clear);
                graphics.DrawText(xCol1, 400, "Moon Phase", Color.Black, font: fontSmall);
                graphics.DrawText(xCol1, 412, GetMoonPhaseName(weather.MoonPhase), Color.Black, font: fontMedium);

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

    private static WeatherData GetSampleWeatherData()
    {
        var now = DateTime.Now;

        // Load location from configuration for accurate sunrise/sunset calculation
        var config = ConfigurationLoader.LoadConfiguration();
        var locationConfig = config.GetSection("Location").Get<LocationConfiguration>();
        double lat = locationConfig?.Latitude ?? 49.208979;
        double lng = locationConfig?.Longitude ?? -123.809392;

        // Calculate actual sunrise/sunset times using SunTimesCalculator
        var (sunrise, sunset) = SunTimesCalculator.GetSunTimes(now.Date, lat, lng);

        return new WeatherData
        {
            Temperature = 22,
            FeelsLike = 20,
            Humidity = 65,
            Pressure = 1.01,
            WindSpeed = 6.17, // ~12 knots in m/s
            CloudCover = 40,
            Visibility = 10,
            UvIndex = 5,
            Description = "partly cloudy",
            IconCode = "02d",
            Sunrise = sunrise.ToLocalTime(),
            Sunset = sunset.ToLocalTime(),
            Timestamp = now,
            MoonPhase = MoonPhaseCalculator.GetMoonPhase(DateTime.UtcNow)
        };
    }

    private static string GetMoonPhaseName(MoonPhase phase)
    {
        return phase switch
        {
            MoonPhase.NewMoon => "New",
            MoonPhase.WaxingCrescent => "Waxing",
            MoonPhase.FirstQuarter => "1st Qtr",
            MoonPhase.WaxingGibbous => "Waxing",
            MoonPhase.FullMoon => "Full",
            MoonPhase.WaningGibbous => "Waning",
            MoonPhase.LastQuarter => "Last Qtr",
            MoonPhase.WaningCrescent => "Waning",
            _ => "Unknown"
        };
    }
}

//<!=SNOP=>