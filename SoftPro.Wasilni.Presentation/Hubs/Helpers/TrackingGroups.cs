namespace SoftPro.Wasilni.Presentation.Hubs.Helpers;

public static class TrackingGroups
{
    public const string Admin = "admin";

    public static string Bus(int busId)   => $"bus-{busId}";
    public static string Line(int lineId) => $"line-{lineId}";
}
