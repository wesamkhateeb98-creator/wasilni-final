using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBookingService
{
    // ─── Admin ────────────────────────────────────────────────────────────────
    /// <summary>Returns paged bookings filtered by optional status and/or lineId, ordered by Date.</summary>
    Task<Page<GetAdminBookingModel>> GetBookingsForAdminAsync(GetAdminBookingsFilterModel filter, CancellationToken cancellationToken);

    // ─── Driver ───────────────────────────────────────────────────────────────
    /// <summary>Returns all waiting bookings on the driver's current line.</summary>
    Task<List<GetBookingModel>> GetBookingForLineAsync(int driverId, CancellationToken cancellationToken);

    /// <summary>Marks a booking as PickedUp and increments daily ridership.</summary>
    Task<BookingActionResult> ConfirmBookingAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    /// <summary>Marks a booking as Cancelled (passenger didn't board).</summary>
    Task<BookingActionResult> MarkNoShowAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    // ─── Passenger ────────────────────────────────────────────────────────────
    Task<MyBookingResult?> GetMyBookingAsync(int passengerId, CancellationToken cancellationToken);
    Task<AddBookingResult> AddBookingAsync(CreateBookingModel model, CancellationToken cancellationToken);
    Task<BookingActionResult> CancelBookingAsync(int passengerId, CancellationToken cancellationToken);
}
