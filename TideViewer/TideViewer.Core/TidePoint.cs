using System;

namespace TideViewer;

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