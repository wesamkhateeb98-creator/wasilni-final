using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Trips;

public record GetTripModel(
    int Id,
    int BusId,
    string BusPlate,
    int LineId,
    string LineName,
    TripStatus Status,
    double? Latitude,
    double? Longitude,
    int AnonymousCount,
    DateTime StartedAt);
