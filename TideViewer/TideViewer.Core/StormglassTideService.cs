using Meadow.Foundation.Serialization; // <-- MicroJson
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TideViewer;


public class StormglassTideService
{
    private static readonly HttpClient http = new HttpClient();
    private readonly string apiKey;

    // ==== Embedded debug cache you can paste over as needed ====
    private static TideCache EmbeddedCache = new TideCache
    {
        Lat = 49.208979,
        Lng = -123.809392,
        StartLocal = new DateTime(2025, 9, 16, 21, 43, 56),
        EndLocal = new DateTime(2025, 9, 17, 21, 43, 56),
        FetchedAtLocal = new DateTime(2025, 9, 16, 21, 43, 57),
        Points = new[]
        {
            new TidePoint(new DateTime(2025, 9, 17, 0, 0, 0), 0.72178),
            new TidePoint(new DateTime(2025, 9, 17, 1, 0, 0), 0.75459),
            new TidePoint(new DateTime(2025, 9, 17, 2, 0, 0), 1.11549),
            new TidePoint(new DateTime(2025, 9, 17, 3, 0, 0), 1.60761),
            new TidePoint(new DateTime(2025, 9, 17, 4, 0, 0), 1.90289),
            new TidePoint(new DateTime(2025, 9, 17, 5, 0, 0), 1.70604),
            new TidePoint(new DateTime(2025, 9, 17, 6, 0, 0), 0.95144),
            new TidePoint(new DateTime(2025, 9, 17, 7, 0, 0), -0.32808),
            new TidePoint(new DateTime(2025, 9, 17, 8, 0, 0), -1.9685),
            new TidePoint(new DateTime(2025, 9, 17, 9, 0, 0), -3.77297),
            new TidePoint(new DateTime(2025, 9, 17, 10, 0, 0), -5.44619),
            new TidePoint(new DateTime(2025, 9, 17, 11, 0, 0), -6.56168),
            new TidePoint(new DateTime(2025, 9, 17, 12, 0, 0), -6.82415),
            new TidePoint(new DateTime(2025, 9, 17, 13, 0, 0), -6.13517),
            new TidePoint(new DateTime(2025, 9, 17, 14, 0, 0), -4.6916),
            new TidePoint(new DateTime(2025, 9, 17, 15, 0, 0), -2.59186),
            new TidePoint(new DateTime(2025, 9, 17, 16, 0, 0), -0.26247),
            new TidePoint(new DateTime(2025, 9, 17, 17, 0, 0), 1.80446),
            new TidePoint(new DateTime(2025, 9, 17, 18, 0, 0), 3.28084),
            new TidePoint(new DateTime(2025, 9, 17, 19, 0, 0), 4.03543),
            new TidePoint(new DateTime(2025, 9, 17, 20, 0, 0), 4.06824),
            new TidePoint(new DateTime(2025, 9, 17, 21, 0, 0), 3.41207),
            new TidePoint(new DateTime(2025, 9, 17, 22, 0, 0), 2.29659),
            new TidePoint(new DateTime(2025, 9, 17, 23, 0, 0), 1.04987),
            new TidePoint(new DateTime(2025, 9, 18, 0, 0, 0), 0.13123),
        }
    };

    public StormglassTideService(string apiKey)
    {
        this.apiKey = apiKey;
    }

    /// Fetch directly from Stormglass (uses MicroJson)
    public async Task<List<TidePoint>> GetSeaLevelAsync(
        double lat, double lng, DateTime startLocal, DateTime endLocal)
    {
        string start = startLocal.ToUniversalTime().ToString("o");
        string end = endLocal.ToUniversalTime().ToString("o");

        string url = $"https://api.stormglass.io/v2/tide/sea-level/point?lat={lat:F6}&lng={lng:F6}&start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Authorization", apiKey);

        var res = await http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        string json = await res.Content.ReadAsStringAsync();

        var dto = MicroJson.Deserialize<SeaLevelResponse>(json);

        if (dto == null || dto.data == null || dto.data.Length == 0)
        {
            return new List<TidePoint>();
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

    /// Prefer embedded cache if it matches; otherwise fetch & update cache.
    public List<TidePoint> GetSeaLevelCached()
    {
        return EmbeddedCache.Points.OrderBy(p => p.Time).ToList();
    }

    async Task UpdateCache(double lat, double lng, DateTime startLocal, DateTime endLocal)
    {
        var fresh = await GetSeaLevelAsync(lat, lng, startLocal, endLocal);

        // Update in-memory cache so GenerateEmbeddedCacheSnippet can use it
        EmbeddedCache = new TideCache
        {
            Lat = lat,
            Lng = lng,
            StartLocal = startLocal,
            EndLocal = endLocal,
            FetchedAtLocal = DateTime.Now,
            Points = fresh.ToArray()
        };

    }

    /// Emit a C# initializer you can paste into the EmbeddedCache block above.
    public static string GenerateEmbeddedCacheSnippet(TideCache cache, string indent = "        ")
    {
        var sb = new StringBuilder();
        sb.AppendLine("new TideCache");
        sb.AppendLine(indent + "{");
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}Lat = {1:F6}, Lng = {2:F6},", indent, cache.Lat, cache.Lng));
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "{0}StartLocal = new DateTime({1}, {2}, {3}, {4}, {5}, {6}),",
            indent, cache.StartLocal.Year, cache.StartLocal.Month, cache.StartLocal.Day,
            cache.StartLocal.Hour, cache.StartLocal.Minute, cache.StartLocal.Second));
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "{0}EndLocal   = new DateTime({1}, {2}, {3}, {4}, {5}, {6}),",
            indent, cache.EndLocal.Year, cache.EndLocal.Month, cache.EndLocal.Day,
            cache.EndLocal.Hour, cache.EndLocal.Minute, cache.EndLocal.Second));
        sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "{0}FetchedAtLocal = new DateTime({1}, {2}, {3}, {4}, {5}, {6}),",
            indent, cache.FetchedAtLocal.Year, cache.FetchedAtLocal.Month, cache.FetchedAtLocal.Day,
            cache.FetchedAtLocal.Hour, cache.FetchedAtLocal.Minute, cache.FetchedAtLocal.Second));
        sb.AppendLine(indent + "Points = new[]");
        sb.AppendLine(indent + "{");
        foreach (var p in cache.Points.OrderBy(p => p.Time))
        {
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0}    new TidePoint(new DateTime({1}, {2}, {3}, {4}, {5}, {6}), {7:0.#####}),",
                indent, p.Time.Year, p.Time.Month, p.Time.Day, p.Time.Hour, p.Time.Minute, p.Time.Second, p.Level));
        }
        sb.AppendLine(indent + "}");
        sb.AppendLine(indent + "};");
        return sb.ToString();
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