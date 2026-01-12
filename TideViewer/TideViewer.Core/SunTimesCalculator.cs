using System;

namespace TideViewer;

/// <summary>
/// Calculator for sunrise and sunset times
/// Uses a simplified astronomical algorithm
/// </summary>
public static class SunTimesCalculator
{
    /// <summary>
    /// Calculates sunrise and sunset times for a given date and location
    /// </summary>
    /// <param name="date">Date to calculate for (time component is ignored)</param>
    /// <param name="latitude">Latitude (-90 to 90)</param>
    /// <param name="longitude">Longitude (-180 to 180)</param>
    /// <returns>Tuple of (Sunrise, Sunset) in UTC</returns>
    public static (DateTime Sunrise, DateTime Sunset) GetSunTimes(DateTime date, double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentException($"Latitude must be between -90 and 90, got {latitude}", nameof(latitude));
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentException($"Longitude must be between -180 and 180, got {longitude}", nameof(longitude));
        }

        // Julian day calculation
        int a = (14 - date.Month) / 12;
        int y = date.Year + 4800 - a;
        int m = date.Month + 12 * a - 3;
        double jd = date.Day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
        double n = jd - 2451545.0 + 0.0008;

        // Mean solar time
        double jStar = n - longitude / 360.0;

        // Solar mean anomaly
        double M = (357.5291 + 0.98560028 * jStar) % 360;
        double Mrad = M * Math.PI / 180.0;

        // Equation of center
        double C = 1.9148 * Math.Sin(Mrad) + 0.0200 * Math.Sin(2 * Mrad) + 0.0003 * Math.Sin(3 * Mrad);

        // Ecliptic longitude
        double lambda = (M + C + 180 + 102.9372) % 360;
        double lambdaRad = lambda * Math.PI / 180.0;

        // Solar transit
        double jTransit = 2451545.0 + jStar + 0.0053 * Math.Sin(Mrad) - 0.0069 * Math.Sin(2 * lambdaRad);

        // Declination of the sun
        double delta = Math.Asin(Math.Sin(lambdaRad) * Math.Sin(23.44 * Math.PI / 180.0));

        // Hour angle
        double latRad = latitude * Math.PI / 180.0;
        double cosOmega = (Math.Sin(-0.833 * Math.PI / 180.0) - Math.Sin(latRad) * Math.Sin(delta)) / (Math.Cos(latRad) * Math.Cos(delta));

        // Handle polar day/night
        if (cosOmega > 1 || cosOmega < -1)
        {
            // Sun doesn't rise or set - use noon and midnight as approximations
            DateTime noon = date.Date.AddHours(12);
            return (noon.AddHours(-6), noon.AddHours(6));
        }

        double omega = Math.Acos(cosOmega);
        double omegaDeg = omega * 180.0 / Math.PI;

        // Calculate sunrise and sunset Julian days
        double jRise = jTransit - omegaDeg / 360.0;
        double jSet = jTransit + omegaDeg / 360.0;

        // Convert back to DateTime
        DateTime sunrise = JulianToDateTime(jRise);
        DateTime sunset = JulianToDateTime(jSet);

        return (sunrise, sunset);
    }

    private static DateTime JulianToDateTime(double julianDay)
    {
        double jd = julianDay + 0.5;
        int z = (int)jd;
        double f = jd - z;

        int a = z < 2299161 ? z : (int)((z - 1867216.25) / 36524.25) + z + 1 + (int)((z - 1867216.25) / 36524.25) / 4 - (int)((z - 1867216.25) / 36524.25) / 100;
        int b = a + 1524;
        int c = (int)((b - 122.1) / 365.25);
        int d = (int)(365.25 * c);
        int e = (int)((b - d) / 30.6001);

        int day = b - d - (int)(30.6001 * e);
        int month = e < 14 ? e - 1 : e - 13;
        int year = month > 2 ? c - 4716 : c - 4715;

        double hours = f * 24.0;
        int hour = (int)hours;
        double minutes = (hours - hour) * 60.0;
        int minute = (int)minutes;
        double seconds = (minutes - minute) * 60.0;
        int second = (int)seconds;

        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }
}
