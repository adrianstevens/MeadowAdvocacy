using System;

namespace RoadTrip
{
    public interface ITripSource
    {
        event EventHandler<TripReading>? ReadingUpdated;
        void Start();
        void Stop();
    }
}
