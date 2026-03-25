using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Bus;

public record RegisterBusRequest(string Plate, string Color, int LineId, BusType Type, int AccountId, int NumberOfSeats, TimeSpan EstimatedTime);
