using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class DailyRidershipRepository(AppDbContext dbContext)
    : Repository<DailyRidershipEntity>(dbContext), IDailyRidershipRepository
{
    public async Task<int> IncrementAsync(int busId, int lineId, DateOnly day, CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                DailyRidershipEntity? ridership = await dbContext.DailyRiderships
                    .FirstOrDefaultAsync(r => r.BusId == busId && r.Day == day, cancellationToken);

                if (ridership is null)
                {
                    ridership = DailyRidershipEntity.Create(busId, lineId, day);
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

    public Task<List<DailyRidershipEntity>> GetDailyAsync(
        DateOnly from, DateOnly to, int? lineId, CancellationToken cancellationToken)
    {
        var query = dbContext.DailyRiderships
            .Where(r => r.Day >= from && r.Day <= to);

        if (lineId.HasValue) query = query.Where(r => r.LineId == lineId.Value);

        return query.OrderBy(r => r.Day).ToListAsync(cancellationToken);
    }

    public async Task<List<(int Year, int Month, int TotalRiders)>> GetMonthlyAsync(
        int fromYear, int fromMonth, int toYear, int toMonth,
        int? lineId, CancellationToken cancellationToken)
    {
        var from = new DateOnly(fromYear, fromMonth, 1);
        var to   = new DateOnly(toYear, toMonth, 1).AddMonths(1).AddDays(-1);

        var query = dbContext.DailyRiderships
            .Where(r => r.Day >= from && r.Day <= to);

        if (lineId.HasValue) query = query.Where(r => r.LineId == lineId.Value);

        var grouped = await query
            .GroupBy(r => new { r.Day.Year, r.Day.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(r => r.NumberOfRiders) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.Year, g.Month, g.Total)).ToList();
    }

    public async Task<List<(int Year, int TotalRiders)>> GetYearlyAsync(
        int fromYear, int toYear, int? lineId, CancellationToken cancellationToken)
    {
        var query = dbContext.DailyRiderships
            .Where(r => r.Day.Year >= fromYear && r.Day.Year <= toYear);

        if (lineId.HasValue) query = query.Where(r => r.LineId == lineId.Value);

        var grouped = await query
            .GroupBy(r => r.Day.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(r => r.NumberOfRiders) })
            .OrderBy(g => g.Year)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => (g.Year, g.Total)).ToList();
    }
}
