using System;

namespace AnchorMinder
{
    public class AnchorService
    {
        const double EarthRadiusMetres = 6_371_000;

        public GeoPosition? AnchorPosition { get; private set; }
        public GeoPosition? CurrentPosition { get; private set; }
        public double AnchorRadiusMetres { get; set; } = 25.0;
        public bool IsAnchored => AnchorPosition is not null;
        public bool IsDragging => IsAnchored && DistanceMetres > AnchorRadiusMetres;

        public double DistanceMetres { get; private set; }
        public double BearingDegrees { get; private set; }

        public event EventHandler? StateChanged;

        public void DropAnchor(GeoPosition position)
        {
            AnchorPosition = position;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void WeighAnchor()
        {
            AnchorPosition = null;
            DistanceMetres = 0;
            BearingDegrees = 0;
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UpdatePosition(GeoPosition position)
        {
            CurrentPosition = position;

            if (AnchorPosition is not null)
            {
                DistanceMetres = Haversine(AnchorPosition, position);
                BearingDegrees = Bearing(AnchorPosition, position);
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        static double Haversine(GeoPosition from, GeoPosition to)
        {
            var lat1 = ToRadians(from.Latitude);
            var lat2 = ToRadians(to.Latitude);
            var dLat = ToRadians(to.Latitude - from.Latitude);
            var dLon = ToRadians(to.Longitude - from.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1) * Math.Cos(lat2)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return EarthRadiusMetres * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        static double Bearing(GeoPosition from, GeoPosition to)
        {
            var lat1 = ToRadians(from.Latitude);
            var lat2 = ToRadians(to.Latitude);
            var dLon = ToRadians(to.Longitude - from.Longitude);

            var x = Math.Sin(dLon) * Math.Cos(lat2);
            var y = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

            return (ToDegrees(Math.Atan2(x, y)) + 360) % 360;
        }

        static double ToRadians(double deg) => deg * Math.PI / 180;
        static double ToDegrees(double rad) => rad * 180 / Math.PI;
    }
}
