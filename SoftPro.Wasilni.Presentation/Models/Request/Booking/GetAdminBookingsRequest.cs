using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.Models.Request.Booking;

public record GetAdminBookingsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    BookingStatus? Status = null,
    int? LineId = null);
