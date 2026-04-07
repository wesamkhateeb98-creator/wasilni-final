using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Bus;

public record GetBusesForAdminRequest(string? Plate, BusType? Type, int PageNumber, int PageSize);
