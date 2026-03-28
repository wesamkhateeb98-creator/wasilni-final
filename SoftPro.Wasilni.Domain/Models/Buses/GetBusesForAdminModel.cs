using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record GetBusesForAdminModel(
    int BusId,
    string Plate,
    string Color,
    BusType Type,
    int NumberOfSeats,
    int? LineId,
    UsernameModel? Driver
    );