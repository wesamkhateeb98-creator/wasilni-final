using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Buses;

namespace SoftPro.Wasilni.Presentation.Models.Response.Bus;

public record GetBusesResponse(
    int BusId,
    UsernameModel? Owner,
    string Plate,
    string Color,
    BusType Type,
    LineBusResponse? Line,
    UsernameModel? Driver
    );

public record LineBusResponse(int Id, string Name);