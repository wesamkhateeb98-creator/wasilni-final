namespace SoftPro.Wasilni.Presentation.Hubs.Helpers;

public static class TrackingGroups
{
    public const string Admin = "admin";

    public static string Trip(int tripId) => $"trip-{tripId}";
    public static string Line(int lineId) => $"line-{lineId}";
}
