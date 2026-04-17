using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class DailyRidershipRepository(AppDbContext dbContext)
    : Repository<DailyRidershipEntity>(dbContext), IDailyRidershipRepository
{
    public Task<DailyRidershipEntity?> GetOrCreateAsync(IncrementRidershipModel model, CancellationToken cancellationToken)
        => dbContext.DailyRiderships
            .FirstOrDefaultAsync(
                r => r.LineId == model.LineId && r.BusId == model.BusId && r.Day == model.Day,
                cancellationToken);

    public Task<List<DailyRidershipEntity>> GetDailyAsync(GetDailyFilterModel filter, CancellationToken cancellationToken)
    {
        var query = dbContext.DailyRiderships
            .Where(r => r.Day >= filter.From && r.Day <= filter.To);

        if (filter.LineId.HasValue) query = query.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue) query = query.Where(r => r.BusId == filter.BusId.Value);

        return query.OrderBy(r => r.Day).ToListAsync(cancellationToken);
    }

    public async Task<List<MonthlyRidershipResult>> GetMonthlyAsync(GetMonthlyFilterModel filter, CancellationToken cancellationToken)
    {
        var from = new DateOnly(filter.FromYear, filter.FromMonth, 1);
        var to = new DateOnly(filter.ToYear, filter.ToMonth, 1).AddMonths(1).AddDays(-1);

        var query = dbContext.DailyRiderships
            .Where(r => r.Day >= from && r.Day <= to);

        if (filter.LineId.HasValue) query = query.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue) query = query.Where(r => r.BusId == filter.BusId.Value);

        var grouped = await query
            .GroupBy(r => new { r.Day.Year, r.Day.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(r => r.NumberOfRiders) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => new MonthlyRidershipResult(g.Year, g.Month, g.Total)).ToList();
    }

    public async Task<List<YearlyRidershipResult>> GetYearlyAsync(GetYearlyFilterModel filter, CancellationToken cancellationToken)
    {
        var query = dbContext.DailyRiderships
            .Where(r => r.Day.Year >= filter.FromYear && r.Day.Year <= filter.ToYear);

        if (filter.LineId.HasValue) query = query.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue) query = query.Where(r => r.BusId == filter.BusId.Value);

        var grouped = await query
            .GroupBy(r => r.Day.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(r => r.NumberOfRiders) })
            .OrderBy(g => g.Year)
            .ToListAsync(cancellationToken);

        return grouped.Select(g => new YearlyRidershipResult(g.Year, g.Total)).ToList();
    }
}
