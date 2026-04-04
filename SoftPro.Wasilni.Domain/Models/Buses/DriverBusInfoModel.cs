using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record DriverBusInfoModel(
    int BusId,
    string Plate,
    string Color,
    BusType Type,
    BusStatus Status,
    int? LineId,
    string? LineName);
