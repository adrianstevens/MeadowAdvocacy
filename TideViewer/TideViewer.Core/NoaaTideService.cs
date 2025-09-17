using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TideViewer;

public enum TideUnits { English, Metric }
public enum NoaaTimeZone { Gmt, Local }


public class TidePoint
{
    public DateTime Time { get; }
    public double Level { get; }
    public TidePoint(DateTime time, double level)
    {
        Time = time;
        Level = level;
    }
}


public class NoaaTideService
{
    static readonly HttpClient http = new();

    public async Task<List<TidePoint>> GetNoaaPredictionsAsync(
        string stationId,
        DateTime dateLocal,
        int hours,
        TideUnits units,
        string datum = "MLLW",
        NoaaTimeZone timeZone = NoaaTimeZone.Local)
    {
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
            "https://api.tidesandcurrents.noaa.gov/api/prod/datagetter" +
            "?product=predictions" +
            "&application=MeadowTideGraph" +
            $"&begin_date={Uri.EscapeDataString(beginString)}" +
            $"&end_date={Uri.EscapeDataString(endString)}" +
            $"&datum={datum}" +
            $"&station={stationId}" +
            $"&time_zone={tz}" +
            $"&units={unit}" +
            "&interval=6" + // 6-minute resolution
            "&format=json";

        var json = await http.GetStringAsync(url);
        var result = MicroJson.Deserialize<NoaaPredictionResponse>(json);
        if (result?.Predictions == null || result.Predictions.Count == 0)
        {
            throw new Exception("No predictions received. Check station id and dates.");
        }

        var points = result.Predictions
            .Select(p => new TidePoint(time: ParseNoaaTime(p.T), level: double.Parse(p.V, CultureInfo.InvariantCulture)))
            .OrderBy(p => p.Time)
            .ToList();

        return points;
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
