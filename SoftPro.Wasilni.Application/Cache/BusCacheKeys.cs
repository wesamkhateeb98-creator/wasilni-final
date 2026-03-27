namespace SoftPro.Wasilni.Application.Cache;

public static class BusCacheKeys
{
    public static string Location(int busId)     => $"bus-location:{busId}";
    public static string DriverBus(int driverId) => $"driver-bus:{driverId}";
    public static string DriverLine(int driverId) => $"driver-line:{driverId}";
}
