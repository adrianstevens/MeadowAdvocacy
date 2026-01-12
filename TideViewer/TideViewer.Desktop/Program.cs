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

        // Will be set dynamically based on actual weather conditions
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
        DailyForecast[] forecast = new DailyForecast[0];
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
                    w.AirQualityIndex = await weatherService.GetAirQualityIndexAsync(lat, lng);
                    w.MoonPhase = MoonPhaseCalculator.GetMoonPhase(DateTime.UtcNow);
                    // Precipitation chance not available in current weather, using forecast
                    w.PrecipitationChance = 0; // Will get from forecast if available
                    return w;
                }).Result;

                forecast = Task.Run(async () => await weatherService.GetForecastAsync(lat, lng)).Result;
                Console.WriteLine($"Successfully fetched weather data and {forecast.Length} days of forecast from OpenWeatherMap");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch weather data: {ex.Message}. Using sample data.");
                weather = null;
                forecast = new DailyForecast[0];
            }
        }

        // Fall back to sample weather data if not available
        if (weather == null)
        {
            weather = GetSampleWeatherData();
            forecast = GetSampleForecastData();
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

                // Dynamic weather icon based on current conditions (large version)
                var currentWeatherIconType = GetWeatherIcon(weather.IconCode, smallVersion: false);
                var currentWeatherIcon = Resources.GetDitheredIcon(currentWeatherIconType);
                graphics.DrawBuffer(20, 20, currentWeatherIcon);

                // Temperature display (moved down for better spacing)
                string tempUnit = weatherConfig?.Units == "imperial" ? "°F" : "°C";
                graphics.DrawText(150, 30, $"{weather.Temperature:F0}", Color.Black, ScaleFactor.X2, font: fontLarge);
                graphics.DrawText(230, 36, tempUnit, Color.Black, ScaleFactor.X1, font: fontMedium);
                graphics.DrawText(150, 78, $"Feels like {weather.FeelsLike:F0}°", Color.Black, ScaleFactor.X1, font: fontSmall);

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

                // Weather Forecast - horizontal display above tide graph
                if (forecast != null && forecast.Length > 0)
                {
                    int forecastY = 85;
                    int forecastStartX = 274;
                    int maxDays = Math.Min(4, forecast.Length);
                    int columnWidth = (display!.Width - forecastStartX) / maxDays;

                    for (int i = 0; i < maxDays; i++)
                    {
                        var day = forecast[i];
                        int columnX = forecastStartX + (i * columnWidth);

                        // Day name
                        string dayName = day.Date.ToString("ddd").ToUpper();
                        graphics.DrawText(columnX + 5, forecastY, dayName, Color.Black, font: fontSmall);

                        // Weather icon
                        var iconType = GetWeatherIcon(day.IconCode);
                        var weatherIcon = Resources.GetDitheredIcon(iconType);
                        graphics.DrawBuffer(columnX + 5, forecastY + 15, weatherIcon);

                        // Temperature range (moved down to make room for icon)
                        string tempHigh = $"{day.TempMax:F0}°";
                        string tempLow = $"{day.TempMin:F0}°";
                        graphics.DrawText(columnX + 40, forecastY + 20, tempHigh, Color.Black, font: fontMedium);
                        graphics.DrawText(columnX + 40, forecastY + 40, tempLow, Color.Black, ScaleFactor.X1, font: fontSmall);
                    }
                }

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

    private static DailyForecast[] GetSampleForecastData()
    {
        var today = DateTime.Now.Date;
        return new DailyForecast[]
        {
            new DailyForecast
            {
                Date = today.AddDays(1),
                TempMin = 18,
                TempMax = 24,
                Description = "partly cloudy",
                IconCode = "02d",
                Humidity = 60,
                WindSpeed = 5.5
            },
            new DailyForecast
            {
                Date = today.AddDays(2),
                TempMin = 20,
                TempMax = 26,
                Description = "sunny",
                IconCode = "01d",
                Humidity = 55,
                WindSpeed = 4.2
            },
            new DailyForecast
            {
                Date = today.AddDays(3),
                TempMin = 17,
                TempMax = 22,
                Description = "light rain",
                IconCode = "10d",
                Humidity = 75,
                WindSpeed = 6.8
            }
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

    private static IconType GetWeatherIcon(string openWeatherMapIconCode, bool smallVersion = true)
    {
        // OpenWeatherMap icon codes: https://openweathermap.org/weather-conditions
        // 01d/01n = clear sky, 02d/02n = few clouds, 03d/03n = scattered clouds
        // 04d/04n = broken clouds, 09d/09n = shower rain, 10d/10n = rain
        // 11d/11n = thunderstorm, 13d/13n = snow, 50d/50n = mist

        if (smallVersion)
        {
            return openWeatherMapIconCode switch
            {
                "01d" => IconType.sunny_sm,              // clear sky day
                "01n" => IconType.night_clear_sm,        // clear sky night
                "02d" => IconType.partial_sun_sm,        // few clouds day
                "02n" => IconType.night_cloudy_sm,       // few clouds night
                "03d" or "03n" => IconType.cloudy_sm,    // scattered clouds
                "04d" or "04n" => IconType.very_cloudy_sm, // broken clouds
                "09d" or "09n" => IconType.light_rain_sm, // shower rain
                "10d" => IconType.sun_rain_sm,           // rain day
                "10n" => IconType.rain_sm,               // rain night
                "11d" or "11n" => IconType.rain_lighting_sm, // thunderstorm
                "13d" or "13n" => IconType.snow_sm,      // snow
                "50d" or "50n" => IconType.cloudy_sm,    // mist/fog
                _ => IconType.cloudy_sm                   // default fallback
            };
        }
        else
        {
            return openWeatherMapIconCode switch
            {
                "01d" => IconType.sunny,              // clear sky day
                "01n" => IconType.night_clear,        // clear sky night
                "02d" => IconType.partial_sun,        // few clouds day
                "02n" => IconType.night_cloudy,       // few clouds night
                "03d" or "03n" => IconType.cloudy,    // scattered clouds
                "04d" or "04n" => IconType.very_cloudy, // broken clouds
                "09d" or "09n" => IconType.light_rain, // shower rain
                "10d" => IconType.sun_rain,           // rain day
                "10n" => IconType.rain,               // rain night
                "11d" or "11n" => IconType.rain_lighting, // thunderstorm
                "13d" or "13n" => IconType.snow,      // snow
                "50d" or "50n" => IconType.cloudy,    // mist/fog
                _ => IconType.cloudy                   // default fallback
            };
        }
    }
}

//<!=SNOP=>