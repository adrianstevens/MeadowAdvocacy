using Meadow.Foundation.mikroBUS.Sensors.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace RoadTrip
{
    public class TripGpsSource : ITripSource
    {
        public event EventHandler<TripReading>? ReadingUpdated;

        private readonly CGNSS10Spi _gnss;
        private double _speedKph;
        private double _headingDegrees;
        private int _satelliteCount;
        private bool _hasFix;
        private double _lat;
        private double _lon;
        private double _altMeters;

        public TripGpsSource(MikroBusConnector connector)
        {
            _gnss = new CGNSS10Spi(connector);
            _gnss.GgaReceived += OnGgaReceived;
            _gnss.VtgReceived += OnVtgReceived;
        }

        private void OnGgaReceived(object sender, GnssPositionInfo location)
        {
            if (location.NumberOfSatellites.HasValue)
                _satelliteCount = location.NumberOfSatellites.Value;

            _hasFix = location.IsValid && location.Position is not null;

            if (location.Position is not null)
            {
                _lat = location.Position.Latitude;
                _lon = location.Position.Longitude;
                _altMeters = location.Position.Altitude.Meters;
            }

            var reading = _hasFix
                ? new TripReading(_lat, _lon, _altMeters, _speedKph, _headingDegrees, true, _satelliteCount)
                : new TripReading(0, 0, 0, 0, 0, false, _satelliteCount);

            ReadingUpdated?.Invoke(this, reading);
        }

        private void OnVtgReceived(object sender, CourseOverGround course)
        {
            _speedKph = (double)course.Kph;
            _headingDegrees = (double)course.TrueHeading;
        }

        public void Start() => _gnss.StartUpdating();
        public void Stop() => _gnss.StopUpdating();
    }
}
