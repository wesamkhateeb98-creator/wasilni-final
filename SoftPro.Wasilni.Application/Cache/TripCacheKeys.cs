namespace SoftPro.Wasilni.Application.Cache;

public static class TripCacheKeys
{
    public static string Location(int tripId)   => $"bus-location:{tripId}";
    public static string DriverTrip(int driverId) => $"driver-trip:{driverId}";
}
