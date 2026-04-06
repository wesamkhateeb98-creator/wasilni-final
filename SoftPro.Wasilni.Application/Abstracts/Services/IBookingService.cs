using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBookingService
{
    // ─── Driver ───────────────────────────────────────────────────────────────
    /// <summary>Returns all waiting bookings on the driver's current line.</summary>
    Task<List<GetBookingModel>> GetBookingForLineAsync(int driverId, CancellationToken cancellationToken);

    /// <summary>Increments daily ridership for the driver's active bus/line.</summary>
    Task<int> ConfirmRiderAsync(int driverId, CancellationToken cancellationToken);

    /// <summary>Marks a booking as PickedUp and increments daily ridership.</summary>
    Task<BookingActionResult> ConfirmBookingAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    /// <summary>Marks a booking as Cancelled (passenger didn't board).</summary>
    Task<BookingActionResult> MarkNoShowAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    // ─── Passenger ────────────────────────────────────────────────────────────
    Task<MyBookingResult?> GetMyBookingAsync(int passengerId, CancellationToken cancellationToken);
    Task<int> AddBookingAsync(CreateBookingModel model, CancellationToken cancellationToken);
    Task<BookingActionResult> CancelBookingAsync(int passengerId, CancellationToken cancellationToken);
}
