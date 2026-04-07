using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record AddBusModel(string Plate, string Color, int? LineId, BusType Type, Guid key);
