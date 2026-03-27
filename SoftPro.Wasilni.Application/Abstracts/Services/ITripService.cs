using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface ITripService
{
    // ─── Driver ───────────────────────────────────────────────────────────────
    Task<GetTripModel> StartTripAsync(int busId, int driverId, CancellationToken cancellationToken);
    Task EndTripAsync(int tripId, int driverId, CancellationToken cancellationToken);
    Task UpdateLocationAsync(int tripId, double latitude, double longitude, int driverId, CancellationToken cancellationToken);
    Task<int> AdjustAnonymousAsync(int tripId, int delta, int driverId, CancellationToken cancellationToken);
    Task<GetTripModel?> GetMyActiveTripAsync(int driverId, CancellationToken cancellationToken);

    // ─── Passenger ────────────────────────────────────────────────────────────
    Task<List<GetTripModel>> GetActiveTripsAsync(int? lineId, CancellationToken cancellationToken);
    Task<GetBookingModel> AddBookingAsync(int tripId, int passengerId, double latitude, double longitude, CancellationToken cancellationToken);
    Task<int> CancelBookingAsync(int tripId, int passengerId, CancellationToken cancellationToken);
}
