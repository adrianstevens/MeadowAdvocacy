using System;

namespace TideViewer;

public class TideCache
{
    public string Source { get; set; } = "stormglass.io (sea-level)";
    public double Lat { get; set; }
    public double Lng { get; set; }
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public DateTime FetchedAtLocal { get; set; }
    public TidePoint[] Points { get; set; } = Array.Empty<TidePoint>();
}
