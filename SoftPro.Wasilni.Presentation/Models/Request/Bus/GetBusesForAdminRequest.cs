using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Bus;

public record GetBusesForAdminRequest(int? OwnerId, string? Plate, BusTypeFilter Filter, int PageNumber, int PageSize);
