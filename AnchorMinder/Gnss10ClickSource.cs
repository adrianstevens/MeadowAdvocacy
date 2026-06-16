using Meadow;
using Meadow.Foundation.mikroBUS.Sensors.Gnss;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace AnchorMinder
{
    public class Gnss10ClickSource : IGpsSource
    {
        public event EventHandler<GeoPosition>? PositionUpdated;

        public bool HasFix { get; private set; }
        public int SatelliteCount { get; private set; }

        private readonly CGNSS10Spi _gnss;

        public Gnss10ClickSource(MikroBusConnector connector)
        {
            _gnss = new CGNSS10Spi(connector);
            _gnss.GgaReceived += OnGgaReceived;
        }

        private void OnGgaReceived(object sender, GnssPositionInfo location)
        {
            if (location.NumberOfSatellites.HasValue)
                SatelliteCount = location.NumberOfSatellites.Value;

            HasFix = location.IsValid && location.Position is not null;

            if (HasFix)
            {
                PositionUpdated?.Invoke(this, new GeoPosition(
                    location.Position!.Latitude,
                    location.Position.Longitude));
            }
        }

        public void Start() => _gnss.StartUpdating();
        public void Stop() => _gnss.StopUpdating();
    }
}
