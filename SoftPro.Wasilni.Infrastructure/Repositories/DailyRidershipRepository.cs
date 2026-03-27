using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class DailyRidershipRepository(AppDbContext dbContext)
    : Repository<DailyRidershipEntity>(dbContext), IDailyRidershipRepository
{
    public async Task<int> IncrementAsync(int busId, DateOnly day, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                DailyRidershipEntity? ridership = await dbContext.DailyRiderships
                    .FirstOrDefaultAsync(r => r.BusId == busId && r.Day == day, cancellationToken);

                if (ridership is null)
                {
                    ridership = DailyRidershipEntity.Create(busId, day);
                    dbContext.DailyRiderships.Add(ridership);
                }

                ridership.IncrementRiders();
                await dbContext.SaveChangesAsync(cancellationToken);
                return ridership.NumberOfRiders;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Another request updated the row — reload and retry
            }
        }
    }
}
