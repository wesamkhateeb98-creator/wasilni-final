using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Bus;

public record UpdateBusRequest(string Plate, string Color, int? LineId, BusType Type);
