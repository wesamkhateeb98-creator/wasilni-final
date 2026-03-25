using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Bus;

public record GetBusesRequest(int PageNumber, int PageSize, GetBusesFilter Filter);
