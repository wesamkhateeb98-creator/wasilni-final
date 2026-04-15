namespace SoftPro.Wasilni.Application.Cache;

public static class BusCacheKeys
{
    public static string DriverContext(int driverId) => $"driver-context:{driverId}";
    public static string DriverLocation(int driverId) => $"driver-location:{driverId}";
}
