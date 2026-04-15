using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBookingService
{
    // ─── Admin ────────────────────────────────────────────────────────────────
    Task<Page<GetAdminBookingModel>> GetBookingsForAdminAsync(GetAdminBookingsFilterModel filter, CancellationToken cancellationToken);

    // ─── Driver ───────────────────────────────────────────────────────────────
    Task<List<GetBookingModel>> GetBookingForLineAsync(int driverId, CancellationToken cancellationToken);

    Task<BookingActionResult> ConfirmBookingAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    Task<BookingActionResult> MarkNoShowAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    // ─── Passenger ────────────────────────────────────────────────────────────
    Task<MyBookingResult?> GetMyBookingAsync(int passengerId, CancellationToken cancellationToken);
    Task<AddBookingResult> AddBookingAsync(CreateBookingModel model, CancellationToken cancellationToken);
    Task<BookingActionResult> CancelBookingAsync(int passengerId, CancellationToken cancellationToken);
}
