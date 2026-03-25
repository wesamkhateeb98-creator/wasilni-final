using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record GetBusesModel(
    int BusId,
    UsernameModel Owner,
    string Plate,
    string Color,
    BusType Type,
    int LineId,
    UsernameModel? Driver
    );

public record UsernameModel(
    int Id,
    string Name);

public record LineBusModel(int Id, string Name);
