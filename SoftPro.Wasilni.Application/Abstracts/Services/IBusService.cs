using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBusService
{
    // ─── Admin CRUD ───────────────────────────────────────────────────────────
    Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken);
    Task<int> AddAsync(AddBusModel model, CancellationToken cancellationToken);
    Task<int> UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken);
    Task<int> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<int> AddDriverAsync(int busId, int driverId, CancellationToken cancellationToken);
    Task<int> DeleteDriverAsync(int busId, CancellationToken cancellationToken);

    // ─── Driver: Bus state ────────────────────────────────────────────────────
    Task<GetActiveBusModel> ToggleStatusAsync(int driverId, CancellationToken cancellationToken);
    Task<(int BusId, int LineId)> UpdateLocationAsync(int driverId, double latitude, double longitude, CancellationToken cancellationToken);
    Task<(int BusId, int LineId, int Count)> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken);
    Task<int> ConfirmRiderAsync(int driverId, CancellationToken cancellationToken);      // anonymous rider
    Task<GetActiveBusModel?> GetMyActiveBusAsync(int driverId, CancellationToken cancellationToken);

    // ─── Driver: Bookings ─────────────────────────────────────────────────────
    /// <summary>Returns waiting bookings on the driver's line within 40 m of the bus.</summary>
    Task<List<GetBookingModel>> GetNearbyBookingsAsync(int driverId, CancellationToken cancellationToken);

    /// <summary>Marks a booking as PickedUp and increments daily ridership.</summary>
    Task<(int BookingId, int LineId)> ConfirmBookingAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    /// <summary>Marks a booking as Cancelled (passenger didn't board).</summary>
    Task<(int BookingId, int LineId)> MarkNoShowAsync(int bookingId, int driverId, CancellationToken cancellationToken);

    // ─── Passenger ────────────────────────────────────────────────────────────
    Task<List<GetActiveBusModel>> GetActiveBusesAsync(int? lineId, CancellationToken cancellationToken);
    Task<int> AddBookingAsync(int lineId, int passengerId, double latitude, double longitude, CancellationToken cancellationToken);
    Task<(int BookingId, int LineId)> CancelBookingAsync(int passengerId, CancellationToken cancellationToken);
}
