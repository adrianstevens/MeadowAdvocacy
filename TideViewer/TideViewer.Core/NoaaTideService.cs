using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TideViewer.Exceptions;

namespace TideViewer;

public enum TideUnits { English, Metric }
public enum NoaaTimeZone { Gmt, Local }

/// <summary>
/// Service for fetching tide predictions from NOAA Tides and Currents API
/// </summary>
public class NoaaTideService
{
    private static readonly HttpClient http = new();
    private readonly string baseUrl;
    private readonly string applicationName;

    /// <summary>
    /// Creates a new NoaaTideService
    /// </summary>
    /// <param name="baseUrl">Base URL for NOAA API (optional)</param>
    /// <param name="applicationName">Application identifier for NOAA API (optional)</param>
    public NoaaTideService(
        string baseUrl = "https://api.tidesandcurrents.noaa.gov/api/prod/datagetter",
        string applicationName = "MeadowTideGraph")
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException("Application name cannot be null or empty", nameof(applicationName));
        }

        this.baseUrl = baseUrl.TrimEnd('/');
        this.applicationName = applicationName;
    }

    /// <summary>
    /// Fetches tide predictions from NOAA API
    /// </summary>
    /// <param name="stationId">NOAA station ID</param>
    /// <param name="dateLocal">Start date/time in local timezone</param>
    /// <param name="hours">Number of hours to fetch predictions for</param>
    /// <param name="units">English or Metric units</param>
    /// <param name="datum">Vertical datum (default: MLLW - Mean Lower Low Water)</param>
    /// <param name="timeZone">Timezone for returned data</param>
    /// <returns>List of tide points ordered by time</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    /// <exception cref="TideServiceException">Thrown when API call fails</exception>
    public async Task<List<TidePoint>> GetNoaaPredictionsAsync(
        string stationId,
        DateTime dateLocal,
        int hours,
        TideUnits units,
        string datum = "MLLW",
        NoaaTimeZone timeZone = NoaaTimeZone.Local)
    {
        if (string.IsNullOrWhiteSpace(stationId))
        {
            throw new ArgumentException("Station ID cannot be null or empty", nameof(stationId));
        }
        if (hours <= 0)
        {
            throw new ArgumentException("Hours must be positive", nameof(hours));
        }
        if (string.IsNullOrWhiteSpace(datum))
        {
            throw new ArgumentException("Datum cannot be null or empty", nameof(datum));
        }
        // NOAA API returns either 6‑minute predictions or just High/Low when interval=hilo
        // We prefer the 6‑minute series for a smooth curve.
        var begin = dateLocal;
        var end = dateLocal.AddHours(hours);

        string beginString = begin.ToString("yyyyMMdd HH:mm", CultureInfo.InvariantCulture);
        string endString = end.ToString("yyyyMMdd HH:mm", CultureInfo.InvariantCulture);

        string tz = timeZone == NoaaTimeZone.Gmt ? "gmt" : "lst_ldt"; // local standard/local daylight time
        string unit = units == TideUnits.English ? "english" : "metric";


        // Format docs: https://api.tidesandcurrents.noaa.gov/api/prod/datagetter
        // product=predictions gives predicted water level time series.
        string url =
            $"{baseUrl}" +
            "?product=predictions" +
            $"&application={applicationName}" +
            $"&begin_date={Uri.EscapeDataString(beginString)}" +
            $"&end_date={Uri.EscapeDataString(endString)}" +
            $"&datum={datum}" +
            $"&station={stationId}" +
            $"&time_zone={tz}" +
            $"&units={unit}" +
            "&interval=6" + // 6-minute resolution
            "&format=json";

        try
        {
            var json = await http.GetStringAsync(url);
            var result = MicroJson.Deserialize<NoaaPredictionResponse>(json);
            if (result?.Predictions == null || result.Predictions.Count == 0)
            {
                throw new TideServiceException(
                    $"No predictions received from NOAA API. Check station ID '{stationId}' and date range.",
                    "NOAA",
                    url);
            }

            var points = result.Predictions
                .Select(p => new TidePoint(time: ParseNoaaTime(p.T), level: double.Parse(p.V, CultureInfo.InvariantCulture)))
                .OrderBy(p => p.Time)
                .ToList();

            return points;
        }
        catch (HttpRequestException ex)
        {
            throw new TideServiceException(
                $"Failed to fetch tide predictions from NOAA: {ex.Message}",
                ex,
                "NOAA");
        }
        catch (TideServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TideServiceException(
                $"Unexpected error fetching NOAA predictions: {ex.Message}",
                ex,
                "NOAA");
        }
    }

    static DateTime ParseNoaaTime(string t)
    {
        // NOAA time format examples: "2025-09-16 08:24"
        if (DateTime.TryParseExact(t, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeLocal, out var dt))
            return dt;
        // Fallback
        return DateTime.Parse(t, CultureInfo.InvariantCulture);
    }

    class NoaaPredictionResponse
    {
        [JsonPropertyName("predictions")]
        public List<NoaaPrediction> Predictions { get; set; } = new();
    }


    class NoaaPrediction
    {
        [JsonPropertyName("t")] public string T { get; set; } = string.Empty; // time
        [JsonPropertyName("v")] public string V { get; set; } = string.Empty; // value (ft or m)
        [JsonPropertyName("type")] public string? Type { get; set; } // present when interval=hilo
    }
}
