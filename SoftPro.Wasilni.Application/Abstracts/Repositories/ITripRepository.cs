using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface ITripRepository : IRepository<TripEntity>
{
    Task<TripEntity?> GetActiveTripByDriverIdAsync(int driverId, CancellationToken cancellationToken);
    Task<TripEntity?> GetActiveTripByBusIdAsync(int busId, CancellationToken cancellationToken);
    Task<TripEntity?> GetActiveByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<TripEntity>> GetActiveTripsAsync(int? lineId, CancellationToken cancellationToken);
}
