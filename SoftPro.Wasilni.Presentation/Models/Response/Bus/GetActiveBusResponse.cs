namespace SoftPro.Wasilni.Presentation.Models.Response.Bus;

public record GetActiveBusResponse(
    int BusId,
    string Plate,
    int LineId,
    string LineName,
    string Status,
    double? Latitude,
    double? Longitude,
    DateTime? ActiveSince);
