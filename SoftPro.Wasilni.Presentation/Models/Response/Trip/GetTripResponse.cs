namespace SoftPro.Wasilni.Presentation.Models.Response.Trip;

public record GetTripResponse(
    int Id,
    int BusId,
    string BusPlate,
    int LineId,
    string LineName,
    string Status,
    double? Latitude,
    double? Longitude,
    int AnonymousCount,
    DateTime StartedAt);
