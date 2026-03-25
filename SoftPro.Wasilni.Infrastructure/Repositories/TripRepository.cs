using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class TripRepository(AppDbContext dbContext) : Repository<TripEntity>(dbContext), ITripRepository
{
    public Task<TripEntity?> GetActiveTripByDriverIdAsync(int driverId, CancellationToken cancellationToken)
        => dbContext.Trips
            .Include(t => t.Bus)
            .ThenInclude(b => b.LineEntity)
            .FirstOrDefaultAsync(t => t.DriverId == driverId && t.Status == TripStatus.Active, cancellationToken);

    public Task<TripEntity?> GetActiveTripByBusIdAsync(int busId, CancellationToken cancellationToken)
        => dbContext.Trips
            .FirstOrDefaultAsync(t => t.BusId == busId && t.Status == TripStatus.Active, cancellationToken);

    public Task<TripEntity?> GetActiveByIdAsync(int id, CancellationToken cancellationToken)
        => dbContext.Trips
            .FirstOrDefaultAsync(t => t.Id == id && t.Status == TripStatus.Active, cancellationToken);
}
