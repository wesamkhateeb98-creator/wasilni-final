using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Bus;

public record AddBusRequest(string Plate, string Color, int? LineId, BusType Type, Guid key);
