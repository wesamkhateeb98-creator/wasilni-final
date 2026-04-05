namespace SoftPro.Wasilni.Application.Cache;

public static class BusCacheKeys
{
    public static string DriverContext(int driverId) => $"driver-context:{driverId}";
    public static string HasBus(int driverId) => $"has-bus:{driverId}";
    public static string DriverInfo(int driverId) => $"driver-info:{driverId}";
}
