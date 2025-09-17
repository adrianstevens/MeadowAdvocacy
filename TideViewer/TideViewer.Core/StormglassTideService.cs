using Meadow.Foundation.Serialization; // <-- MicroJson
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TideViewer;

public class StormglassTideService
{
    static readonly HttpClient http = new();

    readonly string apiKey;
    public StormglassTideService(string apiKey) { this.apiKey = apiKey; }

    public async Task<List<TidePoint>> GetSeaLevelAsync(
        double lat, double lng, DateTime startLocal, DateTime endLocal)
    {
        // Stormglass expects ISO-8601 UTC
        string start = startLocal.ToUniversalTime().ToString("o");
        string end = endLocal.ToUniversalTime().ToString("o");

        var url =
            $"https://api.stormglass.io/v2/tide/sea-level/point?lat={lat:F6}&lng={lng:F6}&start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}";

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Add("Authorization", apiKey);

        var res = await http.SendAsync(req);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();

        // Response: { "data": [ { "time": "...", "sg": <meters> }, ... ], "meta": { ... } }
        var dto = MicroJson.Deserialize<SeaLevelResponse>(json);
        if (dto?.data == null || dto.data.Length == 0)
            return new List<TidePoint>();

        // Convert meters -> feet (or keep meters if you prefer)
        var points = new List<TidePoint>(dto.data.Length);
        foreach (var p in dto.data)
        {
            // Stormglass time is ISO8601 UTC; convert to local for your plot axis
            var tLocal = DateTime.Parse(p.time, null, DateTimeStyles.AdjustToUniversal).ToLocalTime();
            points.Add(new TidePoint(tLocal, p.sg * 3.28084));
        }

        return points.OrderBy(p => p.Time).ToList();
    }

    // DTOs w/ property names matching JSON (MicroJson uses name matching)
    class SeaLevelResponse
    {
        public SeaLevelPoint[] data { get; set; } = Array.Empty<SeaLevelPoint>();
    }

    class SeaLevelPoint
    {
        public string time { get; set; } = "";
        public double sg { get; set; }
    }
}