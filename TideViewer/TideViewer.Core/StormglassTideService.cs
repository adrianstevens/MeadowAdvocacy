using Meadow.Foundation.Serialization; // <-- MicroJson
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TideViewer.Exceptions;

namespace TideViewer;

/// <summary>
/// Service for fetching tide data from Stormglass.io API
/// </summary>
public class StormglassTideService
{
    private static readonly HttpClient http = new HttpClient();
    private readonly string apiKey;
    private readonly string baseUrl;

    /// <summary>
    /// Creates a new StormglassTideService
    /// </summary>
    /// <param name="apiKey">Stormglass API key</param>
    /// <param name="baseUrl">Base URL for Stormglass API (optional)</param>
    /// <exception cref="ArgumentException">Thrown when apiKey is null or empty</exception>
    public StormglassTideService(string apiKey, string baseUrl = "https://api.stormglass.io")
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
        }
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException("Base URL cannot be null or empty", nameof(baseUrl));
        }

        this.apiKey = apiKey;
        this.baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Fetches sea level data from Stormglass API
    /// </summary>
    /// <param name="lat">Latitude (-90 to 90)</param>
    /// <param name="lng">Longitude (-180 to 180)</param>
    /// <param name="startLocal">Start time in local timezone</param>
    /// <param name="endLocal">End time in local timezone</param>
    /// <returns>List of tide points ordered by time</returns>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    /// <exception cref="TideServiceException">Thrown when API call fails</exception>
    public async Task<List<TidePoint>> GetSeaLevelAsync(
        double lat, double lng, DateTime startLocal, DateTime endLocal)
    {
        if (lat < -90 || lat > 90)
        {
            throw new ArgumentException($"Latitude must be between -90 and 90, got {lat}", nameof(lat));
        }
        if (lng < -180 || lng > 180)
        {
            throw new ArgumentException($"Longitude must be between -180 and 180, got {lng}", nameof(lng));
        }
        if (endLocal <= startLocal)
        {
            throw new ArgumentException("End time must be after start time", nameof(endLocal));
        }

        string start = startLocal.ToUniversalTime().ToString("o");
        string end = endLocal.ToUniversalTime().ToString("o");

        string url = $"{baseUrl}/v2/tide/sea-level/point?lat={lat:F6}&lng={lng:F6}&start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("Authorization", apiKey);

            using var res = await http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            string json = await res.Content.ReadAsStringAsync();

            var dto = MicroJson.Deserialize<SeaLevelResponse>(json);

            if (dto == null || dto.data == null || dto.data.Length == 0)
            {
                throw new TideServiceException(
                    "No sea level data received from Stormglass API. Check coordinates and date range.",
                    "Stormglass",
                    url);
            }

            var points = new List<TidePoint>(dto.data.Length);

            for (int i = 0; i < dto.data.Length; i++)
            {
                var p = dto.data[i];
                // Stormglass time is ISO8601 UTC
                var tLocal = DateTime.Parse(p.time, null, DateTimeStyles.AdjustToUniversal).ToLocalTime();
                // meters
                points.Add(new TidePoint(tLocal, p.sg));
            }

            return points.OrderBy(p => p.Time).ToList();
        }
        catch (HttpRequestException ex)
        {
            throw new TideServiceException(
                $"Failed to fetch sea level data from Stormglass: {ex.Message}",
                ex,
                "Stormglass");
        }
        catch (TideServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TideServiceException(
                $"Unexpected error fetching sea level data: {ex.Message}",
                ex,
                "Stormglass");
        }
    }

    // DTOs for MicroJson (names match JSON)
    private class SeaLevelResponse
    {
        public SeaLevelPoint[] data { get; set; } = Array.Empty<SeaLevelPoint>();
    }

    private class SeaLevelPoint
    {
        public string time { get; set; } = "";
        public double sg { get; set; }   // meters
    }
}