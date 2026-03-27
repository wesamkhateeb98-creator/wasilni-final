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

    public Task<List<TripEntity>> GetActiveTripsAsync(int? lineId, CancellationToken cancellationToken)
    {
        IQueryable<TripEntity> query = dbContext.Trips
            .Include(t => t.Bus)
            .ThenInclude(b => b.LineEntity)
            .Where(t => t.Status == TripStatus.Active);

        if (lineId.HasValue)
            query = query.Where(t => t.LineId == lineId.Value);

        return query.ToListAsync(cancellationToken);
    }
}
