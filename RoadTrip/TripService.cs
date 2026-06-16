using System;

namespace RoadTrip
{
    public class TripService
    {
        public double TotalDistanceKm { get; private set; }
        public double MaxSpeedKph { get; private set; }
        public double AverageSpeedKph => MovingTime.TotalHours > 0 ? TotalDistanceKm / MovingTime.TotalHours : 0;
        public double? MaxAltitudeMeters { get; private set; }
        public double? MinAltitudeMeters { get; private set; }
        public TimeSpan MovingTime { get; private set; }
        public TimeSpan SessionTime => _sessionStart.HasValue ? DateTime.UtcNow - _sessionStart.Value : TimeSpan.Zero;

        private const double MovingThresholdKph = 5.0;
        private DateTime? _sessionStart;
        private DateTime? _lastUpdate;
        private double _lastLat = double.NaN;
        private double _lastLon = double.NaN;

        public void Update(TripReading r)
        {
            var now = DateTime.UtcNow;
            _sessionStart ??= now;

            if (r.SpeedKph > MaxSpeedKph)
                MaxSpeedKph = r.SpeedKph;
            if (!MaxAltitudeMeters.HasValue || r.AltitudeMeters > MaxAltitudeMeters.Value)
                MaxAltitudeMeters = r.AltitudeMeters;
            if (!MinAltitudeMeters.HasValue || r.AltitudeMeters < MinAltitudeMeters.Value)
                MinAltitudeMeters = r.AltitudeMeters;

            if (_lastUpdate.HasValue)
            {
                var elapsed = now - _lastUpdate.Value;
                if (r.SpeedKph >= MovingThresholdKph)
                    MovingTime += elapsed;

                if (!double.IsNaN(_lastLat))
                    TotalDistanceKm += Haversine(_lastLat, _lastLon, r.Latitude, r.Longitude);
            }

            _lastUpdate = now;
            _lastLat = r.Latitude;
            _lastLon = r.Longitude;
        }

        static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0;
            var dLat = (lat2 - lat1) * Math.PI / 180.0;
            var dLon = (lon2 - lon1) * Math.PI / 180.0;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
