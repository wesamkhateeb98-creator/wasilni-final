using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface ITripService
{
    Task<GetTripModel> StartTripAsync(int busId, int driverId, CancellationToken cancellationToken);
    Task EndTripAsync(int tripId, int driverId, CancellationToken cancellationToken);
    Task UpdateLocationAsync(int tripId, double latitude, double longitude, int driverId, CancellationToken cancellationToken);
    Task<int> AdjustAnonymousAsync(int tripId, int delta, int driverId, CancellationToken cancellationToken);
    Task<GetTripModel?> GetMyActiveTripAsync(int driverId, CancellationToken cancellationToken);
}
