namespace RoadTrip
{
    public record TripReading(
        double Latitude,
        double Longitude,
        double AltitudeMeters,
        double SpeedKph,
        double HeadingDegrees,
        bool HasFix,
        int SatelliteCount);
}
