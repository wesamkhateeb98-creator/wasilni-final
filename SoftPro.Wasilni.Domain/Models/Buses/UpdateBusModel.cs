using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record UpdateBusModel(
    string Plate,
    string Color,
    int LineId,
    BusType Type,
    int NumberOfSeats,
    TimeSpan EstimatedTime);
