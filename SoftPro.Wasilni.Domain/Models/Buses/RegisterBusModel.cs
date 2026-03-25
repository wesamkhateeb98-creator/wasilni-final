using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record RegisterBusModel(string Plate, string Color, int lineId, BusType Type, int accountId, int NumberOfSeats , TimeSpan EstimatedTime);
