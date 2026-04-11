using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Domain.Models.Trips;

public record GetAdminBookingsFilterModel(
    int PageNumber,
    int PageSize,
    BookingStatus? Status,
    int? LineId);
