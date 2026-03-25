using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Buses;

public record GetBusModel(int Id, int pageNumber, int PageSize, GetBusesFilter Filter);


public record GetBusForAdminModel(int? OwnerId, string? Plate, BusTypeFilter Filter, int pageNumber, int PageSize);