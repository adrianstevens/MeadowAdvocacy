using System;
using System.Threading;

namespace RoadTrip
{
    public class SimulatedTripSource : ITripSource
    {
        public event EventHandler<TripReading>? ReadingUpdated;

        private Timer? _timer;
        private int _tick;
        private double _lat = 49.2827;    // Vancouver
        private double _lon = -123.1207;
        private double _speed = 95;
        private readonly Random _rng = new Random(42);

        public void Start()
        {
            _timer = new Timer(_ => Tick(), null, 0, 1000);
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        void Tick()
        {
            _tick++;

            // Speed: random walk around 100 km/h, clamped to highway range
            _speed += (_rng.NextDouble() - 0.48) * 6;
            _speed = Math.Clamp(_speed, 80, 125);

            // Heading: mostly south with gentle variation
            double heading = 182 + 6 * Math.Sin(_tick * 0.05);

            // Altitude: rises to ~1300m over the Siskiyous (~5 min in), then flat
            double alt = 15 + Math.Max(0, 1300 * Math.Sin(_tick * 0.008));

            // Move south (one degree latitude ≈ 110.6 km)
            _lat -= (_speed / 3600.0) / 110.6;

            ReadingUpdated?.Invoke(this, new TripReading(
                _lat, _lon, alt, _speed, heading, true, 8));
        }
    }
}
