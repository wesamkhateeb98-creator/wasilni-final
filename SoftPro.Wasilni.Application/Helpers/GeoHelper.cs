namespace SoftPro.Wasilni.Application.Helpers;

/// <summary>
/// Geographic utilities (WGS-84 coordinate system).
/// </summary>
public static class GeoHelper
{
    private const double EarthRadiusMeters = 6_371_000;

    /// <summary>
    /// Returns the great-circle distance in <b>metres</b> between two GPS points
    /// using the Haversine formula.
    /// </summary>
    public static double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = ToRad(lat2 - lat1);
        double dLon = ToRad(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return EarthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180;
}
