using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record GetActiveBusModel(
    int BusId,
    string Plate,
    int LineId,
    string LineName,
    BusStatus Status,
    double? Latitude,
    double? Longitude,
    DateTime? ActiveSince);
