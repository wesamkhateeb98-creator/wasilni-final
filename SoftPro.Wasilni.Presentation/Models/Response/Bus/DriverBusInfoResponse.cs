using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Response.Bus;

public record DriverBusInfoResponse(
    int BusId,
    string Plate,
    string Color,
    BusType Type,
    BusStatus Status,
    int? LineId,
    string? LineName);
