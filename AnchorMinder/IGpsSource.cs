using System;

namespace AnchorMinder
{
    public interface IGpsSource
    {
        event EventHandler<GeoPosition> PositionUpdated;
        void Start();
        void Stop();
    }
}
