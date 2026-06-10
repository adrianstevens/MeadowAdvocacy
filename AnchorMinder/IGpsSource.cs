using System;

namespace AnchorMinder
{
    public interface IGpsSource
    {
        event EventHandler<GeoPosition> PositionUpdated;
        bool HasFix { get; }
        int SatelliteCount { get; }
        void Start();
        void Stop();
    }
}
