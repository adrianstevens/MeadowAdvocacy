using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Peripherals.Displays;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TideViewer;
using TideViewer.assets;
using TideViewer.Models;

namespace TideViewer.Meadow
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        const int DISPLAY_WIDTH = 600;
        const int DISPLAY_HEIGHT = 448;

        // Layout constants (matching Desktop)
        const int COL1_X = 40;
        const int COL2_X = 180;
        const int GRAPH_X = 274;
        const int GRAPH_Y = 200;
        const int GRAPH_W = 322;
        const int GRAPH_H = 245;

        MicroGraphics graphics;
        StormglassTideService tideService;
        OpenWeatherMapService weatherService;

        IFont fontLarge;
        IFont fontMedium;
        IFont fontSmall;
        IFont fontTiny;

        // Configuration values
        double latitude;
        double longitude;
        string locationName;
        string stormglassApiKey;
        string openWeatherMapApiKey;
        string units;

        public override Task Initialize()
        {
            Console.WriteLine("TideViewer initializing...");

            // Read configuration from app.config.yaml via Meadow Settings
            LoadConfiguration();

            // Initialize fonts
            Console.WriteLine("Initializing fonts...");
            fontLarge = new Font12x16();
            fontMedium = new Font8x12();
            fontSmall = new Font6x8();
            fontTiny = new Font4x6();

            // Initialize display
            Console.WriteLine("Creating SPI bus...");
            var spiBus = Device.CreateSpiBus(new global::Meadow.Units.Frequency(48000, global::Meadow.Units.Frequency.UnitType.Kilohertz));
            Console.WriteLine("Creating Epd5in65f display (CS=A04, DC=A03, RST=A02, BUSY=A01)...");
            var display = new Epd5in65f(spiBus, Device.Pins.A04, Device.Pins.A03, Device.Pins.A02, Device.Pins.A01);
            Console.WriteLine($"Display created: {display.Width}x{display.Height}");

            graphics = new MicroGraphics(display)
            {
                CurrentFont = fontMedium,
                Stroke = 1,
            };
            Console.WriteLine("MicroGraphics initialized.");

            // Initialize services
            Console.WriteLine("Creating services...");
            tideService = new StormglassTideService(stormglassApiKey, "https://api.stormglass.io");
            weatherService = new OpenWeatherMapService(
                new Configuration.WeatherServiceConfiguration
                {
                    OpenWeatherMapApiKey = openWeatherMapApiKey,
                    OpenWeatherMapBaseUrl = "https://api.openweathermap.org/data/2.5",
                    Units = units
                });

            Console.WriteLine("TideViewer initialized successfully.");
            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Console.WriteLine("TideViewer Run() starting...");

            while (true)
            {
                try
                {
                    Console.WriteLine("Starting display update cycle...");
                    await UpdateDisplay();
                    Console.WriteLine("Display update cycle complete.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating display: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }

                // ePaper refresh is slow and power-hungry — update every 30 minutes
                Console.WriteLine("Sleeping 30 minutes until next update...");
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }

        private async Task UpdateDisplay()
        {
            // Fetch weather data
            WeatherData weather = null;
            DailyForecast[] forecast = new DailyForecast[0];

            bool useRealWeather = !string.IsNullOrWhiteSpace(openWeatherMapApiKey)
                                  && openWeatherMapApiKey != "dummy-key-for-development";

            if (useRealWeather)
            {
                try
                {
                    weather = await weatherService.GetCurrentWeatherAsync(latitude, longitude);
                    weather.UvIndex = await weatherService.GetUvIndexAsync(latitude, longitude);
                    weather.AirQualityIndex = await weatherService.GetAirQualityIndexAsync(latitude, longitude);
                    weather.PrecipitationChance = await weatherService.GetPrecipitationChanceAsync(latitude, longitude);
                    weather.MoonPhase = MoonPhaseCalculator.GetMoonPhase(DateTime.UtcNow);
                    forecast = await weatherService.GetForecastAsync(latitude, longitude);
                    Console.WriteLine($"Fetched weather + {forecast.Length} day forecast");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Weather API failed: {ex.Message}. Using sample data.");
                    weather = null;
                }
            }

            if (weather == null)
            {
                weather = GetSampleWeatherData();
                forecast = GetSampleForecastData();
            }

            // Fetch tide data
            List<TidePoint> tidePoints = null;
            bool useRealTide = !string.IsNullOrWhiteSpace(stormglassApiKey)
                               && stormglassApiKey != "dummy-key-for-development";

            if (useRealTide)
            {
                try
                {
                    var start = DateTime.UtcNow;
                    var end = start.AddHours(24);
                    tidePoints = await tideService.GetSeaLevelAsync(latitude, longitude, start, end);
                    Console.WriteLine($"Fetched {tidePoints.Count} tide points");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Tide API failed: {ex.Message}. Using sample data.");
                }
            }

            if (tidePoints == null || tidePoints.Count == 0)
            {
                tidePoints = GetSampleTideData();
            }

            // Render
            Console.WriteLine("Rendering to buffer...");
            DrawDisplay(weather, forecast, tidePoints);
            Console.WriteLine("Buffer rendered. Calling Show() to push to ePaper...");
        }

        private void DrawDisplay(WeatherData weather, DailyForecast[] forecast, List<TidePoint> tidePoints)
        {
            graphics.Clear(Color.White);

            // Load icons
            var sunrise = Resources.GetDitheredIcon(IconType.sunrise_sm);
            var wind = Resources.GetDitheredIcon(IconType.windy_sm);
            var uv = Resources.GetDitheredIcon(IconType.sunny_sm);
            var humidity = Resources.GetDitheredIcon(IconType.humidity_sm);
            var pressure = Resources.GetDitheredIcon(IconType.pressure_sm);
            var aqi = Resources.GetDitheredIcon(IconType.aqi_sm);
            var rain = Resources.GetDitheredIcon(IconType.rain_sm);
            var moonIcon = Resources.GetDitheredIcon(IconType.night_clear_sm);

            // Current weather icon (large)
            var currentWeatherIconType = GetWeatherIcon(weather.IconCode, false);
            var currentWeatherIcon = Resources.GetDitheredIcon(currentWeatherIconType);
            graphics.DrawBuffer(20, 20, currentWeatherIcon);

            // Temperature
            string tempUnit = units == "imperial" ? "F" : "C";
            graphics.DrawText(150, 30, $"{weather.Temperature:F0}", Color.Black, ScaleFactor.X2, font: fontLarge);
            graphics.DrawText(230, 36, $"°{tempUnit}", Color.Black, ScaleFactor.X1, font: fontMedium);
            graphics.DrawText(150, 78, $"Feels like {weather.FeelsLike:F0}", Color.Black, ScaleFactor.X1, font: fontSmall);

            // Location and date
            graphics.DrawText(DISPLAY_WIDTH - 4, 4, locationName, Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontLarge);
            graphics.DrawText(DISPLAY_WIDTH - 4, 24, DateTime.Now.ToString("dddd, MMMM d"), Color.Black, ScaleFactor.X1, HorizontalAlignment.Right, font: fontMedium);

            // Sunrise
            graphics.DrawBuffer(4, 200, sunrise);
            graphics.DrawText(COL1_X, 200, "Sunrise", Color.Black, font: fontSmall);
            graphics.DrawText(COL1_X, 210, weather.Sunrise.ToString("h:mmtt").ToLower(), Color.Black, font: fontMedium);

            // Sunset
            graphics.DrawBuffer(144, 200, sunrise);
            graphics.DrawText(COL2_X, 200, "Sunset", Color.Black, font: fontSmall);
            graphics.DrawText(COL2_X, 210, weather.Sunset.ToString("h:mmtt").ToLower(), Color.Black, font: fontMedium);

            // Wind
            graphics.DrawBuffer(4, 240, wind);
            graphics.DrawText(COL1_X, 240, "Wind", Color.Black, font: fontSmall);
            double windKnots = units == "metric" ? weather.WindSpeed * 1.944 : weather.WindSpeed;
            graphics.DrawText(COL1_X, 250, $"{windKnots:F0}kn", Color.Black, font: fontMedium);

            // UV Index
            graphics.DrawBuffer(144, 240, uv);
            graphics.DrawText(COL2_X, 240, "UV Index", Color.Black, font: fontSmall);
            graphics.DrawText(COL2_X, 250, $"{weather.UvIndex:F0}", Color.Black, font: fontMedium);

            // Humidity
            graphics.DrawBuffer(4, 280, humidity);
            graphics.DrawText(COL1_X, 280, "Humidity", Color.Black, font: fontSmall);
            graphics.DrawText(COL1_X, 290, $"{weather.Humidity}%", Color.Black, font: fontMedium);

            // Pressure
            graphics.DrawBuffer(144, 280, pressure);
            graphics.DrawText(COL2_X, 280, "Pressure", Color.Black, font: fontSmall);
            graphics.DrawText(COL2_X, 290, $"{weather.Pressure:F2}atm", Color.Black, font: fontMedium);

            // AQI
            graphics.DrawBuffer(4, 320, aqi);
            graphics.DrawText(COL1_X, 320, "AQI", Color.Black, font: fontSmall);
            graphics.DrawText(COL1_X, 330, GetAqiLabel(weather.AirQualityIndex), Color.Black, font: fontMedium);

            // Precipitation
            graphics.DrawBuffer(144, 320, rain);
            graphics.DrawText(COL2_X, 320, "Precip", Color.Black, font: fontSmall);
            graphics.DrawText(COL2_X, 330, $"{weather.PrecipitationChance}%", Color.Black, font: fontMedium);

            // Moon Phase
            graphics.DrawBuffer(4, 360, moonIcon);
            graphics.DrawText(COL1_X, 360, "Moon", Color.Black, font: fontSmall);
            graphics.DrawText(COL1_X, 370, GetMoonPhaseName(weather.MoonPhase), Color.Black, font: fontMedium);

            // Forecast (horizontal, above tide graph)
            if (forecast != null && forecast.Length > 0)
            {
                int forecastY = 85;
                int forecastStartX = GRAPH_X;
                int maxDays = Math.Min(4, forecast.Length);
                int columnWidth = (DISPLAY_WIDTH - forecastStartX) / maxDays;

                for (int i = 0; i < maxDays; i++)
                {
                    var day = forecast[i];
                    int columnX = forecastStartX + (i * columnWidth);

                    string dayName = day.Date.ToString("ddd").ToUpper();
                    graphics.DrawText(columnX + 5, forecastY, dayName, Color.Black, font: fontSmall);

                    var iconType = GetWeatherIcon(day.IconCode, true);
                    var weatherIcon = Resources.GetDitheredIcon(iconType);
                    graphics.DrawBuffer(columnX + 5, forecastY + 12, weatherIcon);

                    graphics.DrawText(columnX + 40, forecastY + 16, $"{day.TempMax:F0}", Color.Black, font: fontMedium);
                    graphics.DrawText(columnX + 40, forecastY + 30, $"{day.TempMin:F0}", Color.Black, font: fontSmall);
                }
            }

            // Tide graph
            DrawTideGraph(tidePoints, GRAPH_X, GRAPH_Y, GRAPH_X + GRAPH_W, GRAPH_Y + GRAPH_H, "m");

            // Push to ePaper
            Console.WriteLine("Pushing buffer to ePaper display...");
            graphics.Show();
            Console.WriteLine("Show() complete — ePaper should be updating.");
        }

        private void DrawTideGraph(IList<TidePoint> points, int x0, int y0, int x1, int y1, string yUnits)
        {
            int left = Math.Min(x0, x1);
            int top = Math.Min(y0, y1);
            int right = Math.Max(x0, x1);
            int bottom = Math.Max(y0, y1);
            int W = right - left;
            int H = bottom - top;

            graphics.DrawHorizontalGradient(left, top, W, H, Color.White, Color.SkyBlue);

            if (points == null || points.Count < 2)
            {
                graphics.CurrentFont = fontTiny;
                graphics.DrawText(left + 4, top + 4, "no tide data");
                return;
            }

            double minV = points.Min(p => p.Level) - 2.0;
            double maxV = points.Max(p => p.Level) + 2.0;

            if (Math.Abs(maxV - minV) < 0.1)
            {
                maxV += 0.5;
                minV -= 0.5;
            }

            DateTime timeStart = points.First().Time;
            DateTime timeEnd = points.Last().Time;
            double totalMinutes = Math.Max(1, (timeEnd - timeStart).TotalMinutes);

            // Y grid + labels
            graphics.CurrentFont = fontTiny;
            int yGrids = 3;

            for (int i = 0; i <= yGrids; i++)
            {
                int y = top + (int)Math.Round(H * (1.0 - i / (double)yGrids));
                graphics.DrawLine(left, y, left + W, y, Color.Black);
                double v = minV + (maxV - minV) * (i / (double)yGrids);
                graphics.DrawText(left + 2, y - 7, v.ToString("0.0", CultureInfo.InvariantCulture) + yUnits, Color.Black);
            }

            // X ticks (every 6 hours)
            var step = TimeSpan.FromHours(6);
            var tick = RoundUp(timeStart, step);
            for (var t = tick; t <= timeEnd; t = t.Add(step))
            {
                int x = left + (int)Math.Round(W * ((t - timeStart).TotalMinutes / totalMinutes));
                graphics.DrawLine(x, top, x, top + H, Color.Black);
            }

            // Plot tide line
            int? lx = null, ly = null;
            foreach (var p in points)
            {
                int x = left + (int)Math.Round(W * ((p.Time - timeStart).TotalMinutes / totalMinutes));
                int y = top + (int)Math.Round(H * (1.0 - (p.Level - minV) / (maxV - minV)));

                if (lx.HasValue)
                {
                    graphics.DrawLine(lx.Value, ly.Value, x, y, Color.Blue);
                }
                lx = x;
                ly = y;
            }
        }

        private static DateTime RoundUp(DateTime dt, TimeSpan step)
        {
            long ticks = ((dt.Ticks + step.Ticks - 1) / step.Ticks) * step.Ticks;
            return new DateTime(ticks, dt.Kind);
        }

        private void LoadConfiguration()
        {
            // Meadow exposes app.config.yaml as Settings dictionary
            latitude = GetSettingDouble("Location__Latitude", 49.208979);
            longitude = GetSettingDouble("Location__Longitude", -123.809392);
            locationName = GetSetting("Location__Name", "Entrance Island");
            stormglassApiKey = GetSetting("TideService__StormglassApiKey", "");
            openWeatherMapApiKey = GetSetting("WeatherService__OpenWeatherMapApiKey", "");
            units = GetSetting("WeatherService__Units", "metric");

            Console.WriteLine($"Location: {locationName} ({latitude}, {longitude})");
            Console.WriteLine($"Stormglass API key: {(string.IsNullOrEmpty(stormglassApiKey) ? "NOT SET" : "configured")}");
            Console.WriteLine($"OpenWeatherMap API key: {(string.IsNullOrEmpty(openWeatherMapApiKey) ? "NOT SET" : "configured")}");
        }

        private string GetSetting(string key, string defaultValue)
        {
            if (Settings != null && Settings.ContainsKey(key))
            {
                return Settings[key];
            }
            return defaultValue;
        }

        private double GetSettingDouble(string key, double defaultValue)
        {
            var value = GetSetting(key, null);
            if (value != null && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return defaultValue;
        }

        private static string GetAqiLabel(int aqi)
        {
            if (aqi == 1) return "Good";
            if (aqi == 2) return "Fair";
            if (aqi == 3) return "Moderate";
            if (aqi == 4) return "Poor";
            if (aqi == 5) return "Very Poor";
            return "N/A";
        }

        private static string GetMoonPhaseName(MoonPhase phase)
        {
            if (phase == MoonPhase.NewMoon) return "New";
            if (phase == MoonPhase.WaxingCrescent) return "Waxing";
            if (phase == MoonPhase.FirstQuarter) return "1st Qtr";
            if (phase == MoonPhase.WaxingGibbous) return "Waxing";
            if (phase == MoonPhase.FullMoon) return "Full";
            if (phase == MoonPhase.WaningGibbous) return "Waning";
            if (phase == MoonPhase.LastQuarter) return "Last Qtr";
            if (phase == MoonPhase.WaningCrescent) return "Waning";
            return "Unknown";
        }

        private static IconType GetWeatherIcon(string code, bool small)
        {
            if (small)
            {
                if (code == "01d") return IconType.sunny_sm;
                if (code == "01n") return IconType.night_clear_sm;
                if (code == "02d") return IconType.partial_sun_sm;
                if (code == "02n") return IconType.night_cloudy_sm;
                if (code == "03d" || code == "03n") return IconType.cloudy_sm;
                if (code == "04d" || code == "04n") return IconType.very_cloudy_sm;
                if (code == "09d" || code == "09n") return IconType.light_rain_sm;
                if (code == "10d") return IconType.sun_rain_sm;
                if (code == "10n") return IconType.rain_sm;
                if (code == "11d" || code == "11n") return IconType.rain_lighting_sm;
                if (code == "13d" || code == "13n") return IconType.snow_sm;
                if (code == "50d" || code == "50n") return IconType.cloudy_sm;
                return IconType.cloudy_sm;
            }
            else
            {
                if (code == "01d") return IconType.sunny;
                if (code == "01n") return IconType.night_clear;
                if (code == "02d") return IconType.partial_sun;
                if (code == "02n") return IconType.night_cloudy;
                if (code == "03d" || code == "03n") return IconType.cloudy;
                if (code == "04d" || code == "04n") return IconType.very_cloudy;
                if (code == "09d" || code == "09n") return IconType.light_rain;
                if (code == "10d") return IconType.sun_rain;
                if (code == "10n") return IconType.rain;
                if (code == "11d" || code == "11n") return IconType.rain_lighting;
                if (code == "13d" || code == "13n") return IconType.snow;
                if (code == "50d" || code == "50n") return IconType.cloudy;
                return IconType.cloudy;
            }
        }

        private List<TidePoint> GetSampleTideData()
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

        private WeatherData GetSampleWeatherData()
        {
            var now = DateTime.Now;
            var (sunriseUtc, sunsetUtc) = SunTimesCalculator.GetSunTimes(now.Date, latitude, longitude);

            return new WeatherData
            {
                Temperature = 22,
                FeelsLike = 20,
                Humidity = 65,
                Pressure = 1.01,
                WindSpeed = 6.17,
                CloudCover = 40,
                Visibility = 10,
                UvIndex = 5,
                AirQualityIndex = 2,
                PrecipitationChance = 30,
                Description = "partly cloudy",
                IconCode = "02d",
                Sunrise = sunriseUtc.ToLocalTime(),
                Sunset = sunsetUtc.ToLocalTime(),
                Timestamp = now,
                MoonPhase = MoonPhaseCalculator.GetMoonPhase(DateTime.UtcNow)
            };
        }

        private DailyForecast[] GetSampleForecastData()
        {
            var today = DateTime.Now.Date;
            return new DailyForecast[]
            {
                new DailyForecast
                {
                    Date = today.AddDays(1), TempMin = 18, TempMax = 24,
                    Description = "partly cloudy", IconCode = "02d", Humidity = 60, WindSpeed = 5.5
                },
                new DailyForecast
                {
                    Date = today.AddDays(2), TempMin = 20, TempMax = 26,
                    Description = "sunny", IconCode = "01d", Humidity = 55, WindSpeed = 4.2
                },
                new DailyForecast
                {
                    Date = today.AddDays(3), TempMin = 17, TempMax = 22,
                    Description = "light rain", IconCode = "10d", Humidity = 75, WindSpeed = 6.8
                }
            };
        }
    }
}
