using Meadow.Foundation.Serialization;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using TideViewer.Configuration;
using TideViewer.Exceptions;
using TideViewer.Models;

namespace TideViewer;

/// <summary>
/// Service for fetching weather data from OpenWeatherMap API
/// </summary>
public class OpenWeatherMapService
{
    private static readonly HttpClient http = new();
    private readonly string apiKey;
    private readonly string baseUrl;
    private readonly string units;

    /// <summary>
    /// Creates a new OpenWeatherMapService
    /// </summary>
    /// <param name="config">Weather service configuration</param>
    /// <exception cref="ArgumentException">Thrown when API key is null or empty</exception>
    public OpenWeatherMapService(WeatherServiceConfiguration config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }
        if (string.IsNullOrWhiteSpace(config.OpenWeatherMapApiKey))
        {
            throw new ArgumentException("OpenWeatherMap API key cannot be null or empty", nameof(config));
        }

        this.apiKey = config.OpenWeatherMapApiKey;
        this.baseUrl = config.OpenWeatherMapBaseUrl?.TrimEnd('/') ?? "https://api.openweathermap.org/data/2.5";
        this.units = config.Units ?? "metric";
    }

    /// <summary>
    /// Fetches current weather data for specified coordinates
    /// </summary>
    /// <param name="lat">Latitude (-90 to 90)</param>
    /// <param name="lng">Longitude (-180 to 180)</param>
    /// <returns>Current weather data</returns>
    /// <exception cref="ArgumentException">Thrown when coordinates are invalid</exception>
    /// <exception cref="TideServiceException">Thrown when API call fails</exception>
    public async Task<WeatherData> GetCurrentWeatherAsync(double lat, double lng)
    {
        if (lat < -90 || lat > 90)
        {
            throw new ArgumentException($"Latitude must be between -90 and 90, got {lat}", nameof(lat));
        }
        if (lng < -180 || lng > 180)
        {
            throw new ArgumentException($"Longitude must be between -180 and 180, got {lng}", nameof(lng));
        }

        string url = $"{baseUrl}/weather?lat={lat:F6}&lon={lng:F6}&appid={apiKey}&units={units}";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            using var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            string json = await res.Content.ReadAsStringAsync();
            var dto = MicroJson.Deserialize<OpenWeatherMapResponse>(json);

            if (dto == null)
            {
                throw new TideServiceException(
                    "No weather data received from OpenWeatherMap API",
                    "OpenWeatherMap",
                    url);
            }

            return new WeatherData
            {
                Temperature = dto.main?.temp ?? 0,
                FeelsLike = dto.main?.feels_like ?? 0,
                Humidity = dto.main?.humidity ?? 0,
                Pressure = (dto.main?.pressure ?? 0) / 1013.25, // Convert hPa to atm
                WindSpeed = dto.wind?.speed ?? 0,
                CloudCover = dto.clouds?.all ?? 0,
                Visibility = (dto.visibility ?? 0) / 1000.0, // Convert m to km
                Description = dto.weather != null && dto.weather.Length > 0 ? dto.weather[0].description : "",
                IconCode = dto.weather != null && dto.weather.Length > 0 ? dto.weather[0].icon : "",
                Sunrise = DateTimeOffset.FromUnixTimeSeconds(dto.sys?.sunrise ?? 0).LocalDateTime,
                Sunset = DateTimeOffset.FromUnixTimeSeconds(dto.sys?.sunset ?? 0).LocalDateTime,
                UvIndex = 0, // Requires separate API call
                Timestamp = DateTime.Now
            };
        }
        catch (HttpRequestException ex)
        {
            throw new TideServiceException(
                $"Failed to fetch weather data from OpenWeatherMap: {ex.Message}",
                ex,
                "OpenWeatherMap");
        }
        catch (TideServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TideServiceException(
                $"Unexpected error fetching weather data: {ex.Message}",
                ex,
                "OpenWeatherMap");
        }
    }

    /// <summary>
    /// Fetches UV index data for specified coordinates
    /// </summary>
    public async Task<double> GetUvIndexAsync(double lat, double lng)
    {
        string url = $"{baseUrl}/uvi?lat={lat:F6}&lon={lng:F6}&appid={apiKey}";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            using var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            string json = await res.Content.ReadAsStringAsync();
            var dto = MicroJson.Deserialize<UvIndexResponse>(json);

            return dto?.value ?? 0;
        }
        catch
        {
            return 0; // UV index is optional
        }
    }

    /// <summary>
    /// Fetches Air Quality Index for specified coordinates
    /// </summary>
    public async Task<int> GetAirQualityIndexAsync(double lat, double lng)
    {
        string url = $"http://api.openweathermap.org/data/2.5/air_pollution?lat={lat:F6}&lon={lng:F6}&appid={apiKey}";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            using var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            string json = await res.Content.ReadAsStringAsync();
            var dto = MicroJson.Deserialize<AirPollutionResponse>(json);

            // AQI scale: 1=Good, 2=Fair, 3=Moderate, 4=Poor, 5=Very Poor
            return dto?.list != null && dto.list.Length > 0 ? dto.list[0].main?.aqi ?? 0 : 0;
        }
        catch
        {
            return 0; // AQI is optional
        }
    }

    /// <summary>
    /// Fetches 5-day weather forecast for specified coordinates
    /// </summary>
    /// <param name="lat">Latitude (-90 to 90)</param>
    /// <param name="lng">Longitude (-180 to 180)</param>
    /// <returns>List of daily forecasts (up to 5 days)</returns>
    public async Task<DailyForecast[]> GetForecastAsync(double lat, double lng)
    {
        if (lat < -90 || lat > 90)
        {
            throw new ArgumentException($"Latitude must be between -90 and 90, got {lat}", nameof(lat));
        }
        if (lng < -180 || lng > 180)
        {
            throw new ArgumentException($"Longitude must be between -180 and 180, got {lng}", nameof(lng));
        }

        string url = $"{baseUrl}/forecast?lat={lat:F6}&lon={lng:F6}&appid={apiKey}&units={units}";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            using var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();

            string json = await res.Content.ReadAsStringAsync();
            var dto = MicroJson.Deserialize<ForecastResponse>(json);

            if (dto?.list == null || dto.list.Length == 0)
            {
                return new DailyForecast[0];
            }

            // Group forecasts by day and aggregate
            var dailyForecasts = new System.Collections.Generic.List<DailyForecast>();
            DateTime? currentDay = null;
            double tempMin = double.MaxValue;
            double tempMax = double.MinValue;
            string description = "";
            string iconCode = "";
            int humiditySum = 0;
            double windSpeedSum = 0;
            int count = 0;

            foreach (var forecast in dto.list)
            {
                var forecastTime = DateTimeOffset.FromUnixTimeSeconds(forecast.dt).LocalDateTime;
                var forecastDate = forecastTime.Date;

                if (currentDay != forecastDate)
                {
                    // Save previous day's forecast
                    if (currentDay.HasValue && count > 0)
                    {
                        dailyForecasts.Add(new DailyForecast
                        {
                            Date = currentDay.Value,
                            TempMin = tempMin,
                            TempMax = tempMax,
                            Description = description,
                            IconCode = iconCode,
                            Humidity = humiditySum / count,
                            WindSpeed = windSpeedSum / count
                        });
                    }

                    // Start new day
                    currentDay = forecastDate;
                    tempMin = forecast.main?.temp_min ?? 0;
                    tempMax = forecast.main?.temp_max ?? 0;
                    description = forecast.weather != null && forecast.weather.Length > 0 ? forecast.weather[0].description : "";
                    iconCode = forecast.weather != null && forecast.weather.Length > 0 ? forecast.weather[0].icon : "";
                    humiditySum = forecast.main?.humidity ?? 0;
                    windSpeedSum = forecast.wind?.speed ?? 0;
                    count = 1;
                }
                else
                {
                    // Update daily aggregates
                    tempMin = Math.Min(tempMin, forecast.main?.temp_min ?? tempMin);
                    tempMax = Math.Max(tempMax, forecast.main?.temp_max ?? tempMax);
                    humiditySum += forecast.main?.humidity ?? 0;
                    windSpeedSum += forecast.wind?.speed ?? 0;
                    count++;
                }
            }

            // Save last day
            if (currentDay.HasValue && count > 0)
            {
                dailyForecasts.Add(new DailyForecast
                {
                    Date = currentDay.Value,
                    TempMin = tempMin,
                    TempMax = tempMax,
                    Description = description,
                    IconCode = iconCode,
                    Humidity = humiditySum / count,
                    WindSpeed = windSpeedSum / count
                });
            }

            return dailyForecasts.ToArray();
        }
        catch (HttpRequestException ex)
        {
            throw new TideServiceException(
                $"Failed to fetch forecast data from OpenWeatherMap: {ex.Message}",
                ex,
                "OpenWeatherMap");
        }
        catch (TideServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TideServiceException(
                $"Unexpected error fetching forecast data: {ex.Message}",
                ex,
                "OpenWeatherMap");
        }
    }

    // DTOs for MicroJson (names match JSON)
    private class OpenWeatherMapResponse
    {
        public WeatherDescription[]? weather { get; set; }
        public MainData? main { get; set; }
        public Wind? wind { get; set; }
        public Clouds? clouds { get; set; }
        public int? visibility { get; set; }
        public Sys? sys { get; set; }
    }

    private class WeatherDescription
    {
        public string description { get; set; } = "";
        public string icon { get; set; } = "";
    }

    private class MainData
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
        public int humidity { get; set; }
        public double pressure { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }

    private class Wind
    {
        public double speed { get; set; }
    }

    private class Clouds
    {
        public int all { get; set; }
    }

    private class Sys
    {
        public long sunrise { get; set; }
        public long sunset { get; set; }
    }

    private class UvIndexResponse
    {
        public double value { get; set; }
    }

    private class ForecastResponse
    {
        public ForecastItem[]? list { get; set; }
    }

    private class ForecastItem
    {
        public long dt { get; set; }
        public MainData? main { get; set; }
        public WeatherDescription[]? weather { get; set; }
        public Wind? wind { get; set; }
    }

    private class AirPollutionResponse
    {
        public AirPollutionItem[]? list { get; set; }
    }

    private class AirPollutionItem
    {
        public AirQualityMain? main { get; set; }
    }

    private class AirQualityMain
    {
        public int aqi { get; set; }
    }
}
