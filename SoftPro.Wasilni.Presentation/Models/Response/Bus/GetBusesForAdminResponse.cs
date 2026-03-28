using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Buses;


namespace SoftPro.Wasilni.Presentation.Models.Response.Bus;

public record GetBusesForAdminResponse(
    int BusId,
    string Plate,
    string Color,
    BusType Type,
    int NumberOfSeats,
    int? LineId,
    UsernameModel? Driver
    );