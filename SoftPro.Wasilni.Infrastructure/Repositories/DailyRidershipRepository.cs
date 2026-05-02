using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Infrastructure.Persistence;
using SoftPro.Wasilni.Infrastructure.Repositories;

public class DailyRidershipRepository(AppDbContext dbContext)
    : Repository<DailyRidershipEntity>(dbContext), IDailyRidershipRepository
{
    public Task<DailyRidershipEntity?> GetByLineIdAndBusIdAsync(IncrementRidershipModel model, CancellationToken cancellationToken)
        => dbContext.DailyRiderships
            .FirstOrDefaultAsync(
                r => r.LineId == model.LineId && r.BusId == model.BusId && r.Day == model.Day,
                cancellationToken);

    public async Task<List<DailyRidershipData>> GetDailyAsync(GetDailyFilterModel filter, CancellationToken cancellationToken)
    {
        if (HasPassengerFilter(filter))
        {
            var query = dbContext.Bookings
                .Where(b => b.Date >= filter.From && b.Date <= filter.To);

            if (filter.LineId.HasValue) query = query.Where(b => b.LineId == filter.LineId.Value);
            query = ApplyPassengerFilters(query, filter.BeginDateOfBirth, filter.EndDateOfBirth, filter.Gender);

            return await query
                .GroupBy(b => new { b.Date, b.LineId })
                .OrderBy(g => g.Key.Date)
                .Select(g => new DailyRidershipData(g.Key.LineId, 0, g.Key.Date, g.Count()))
                .ToListAsync(cancellationToken);
        }

        var baseQuery = dbContext.DailyRiderships
            .Where(r => r.Day >= filter.From && r.Day <= filter.To);

        if (filter.LineId.HasValue) baseQuery = baseQuery.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue) baseQuery = baseQuery.Where(r => r.BusId == filter.BusId.Value);

        return await baseQuery
            .OrderBy(r => r.Day)
            .Select(r => new DailyRidershipData(r.LineId, r.BusId, r.Day, r.NumberOfRiders))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<MonthlyRidershipResult>> GetMonthlyAsync(GetMonthlyFilterModel filter, CancellationToken cancellationToken)
    {
        var from = new DateOnly(filter.FromYear, filter.FromMonth, 1);
        var to = new DateOnly(filter.ToYear, filter.ToMonth, 1).AddMonths(1).AddDays(-1);

        if (HasPassengerFilter(filter))
        {
            var query = dbContext.Bookings
                .Where(b => b.Date >= from && b.Date <= to);

            if (filter.LineId.HasValue) query = query.Where(b => b.LineId == filter.LineId.Value);
            query = ApplyPassengerFilters(query, filter.BeginDateOfBirth, filter.EndDateOfBirth, filter.Gender);

            var grouped = await query
                .GroupBy(b => new { b.Date.Year, b.Date.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync(cancellationToken);

            return grouped.Select(g => new MonthlyRidershipResult(g.Year, g.Month, g.Total)).ToList();
        }

        var baseQuery = dbContext.DailyRiderships.Where(r => r.Day >= from && r.Day <= to);

        if (filter.LineId.HasValue) baseQuery = baseQuery.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue) baseQuery = baseQuery.Where(r => r.BusId == filter.BusId.Value);

        var results = await baseQuery
            .GroupBy(r => new { r.Day.Year, r.Day.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(r => r.NumberOfRiders) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(cancellationToken);

        return results.Select(g => new MonthlyRidershipResult(g.Year, g.Month, g.Total)).ToList();
    }

    public async Task<List<YearlyRidershipResult>> GetYearlyAsync(GetYearlyFilterModel filter, CancellationToken cancellationToken)
    {
        if (HasPassengerFilter(filter))
        {
            var query = dbContext.Bookings
                .Where(b => b.Date.Year >= filter.FromYear && b.Date.Year <= filter.ToYear);

            if (filter.LineId.HasValue) query = query.Where(b => b.LineId == filter.LineId.Value);
            query = ApplyPassengerFilters(query, filter.BeginDateOfBirth, filter.EndDateOfBirth, filter.Gender);

            var grouped = await query
                .GroupBy(b => b.Date.Year)
                .Select(g => new { Year = g.Key, Total = g.Count() })
                .OrderBy(g => g.Year)
                .ToListAsync(cancellationToken);

            return grouped.Select(g => new YearlyRidershipResult(g.Year, g.Total)).ToList();
        }

        var baseQuery = dbContext.DailyRiderships
            .Where(r => r.Day.Year >= filter.FromYear && r.Day.Year <= filter.ToYear);

        if (filter.LineId.HasValue) baseQuery = baseQuery.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue) baseQuery = baseQuery.Where(r => r.BusId == filter.BusId.Value);

        var results = await baseQuery
            .GroupBy(r => r.Day.Year)
            .Select(g => new { Year = g.Key, Total = g.Sum(r => r.NumberOfRiders) })
            .OrderBy(g => g.Year)
            .ToListAsync(cancellationToken);

        return results.Select(g => new YearlyRidershipResult(g.Year, g.Total)).ToList();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static bool HasPassengerFilter(GetDailyFilterModel f) => f.BeginDateOfBirth.HasValue || f.EndDateOfBirth.HasValue || f.Gender.HasValue;
    private static bool HasPassengerFilter(GetMonthlyFilterModel f) => f.BeginDateOfBirth.HasValue || f.EndDateOfBirth.HasValue || f.Gender.HasValue;
    private static bool HasPassengerFilter(GetYearlyFilterModel f) => f.BeginDateOfBirth.HasValue || f.EndDateOfBirth.HasValue || f.Gender.HasValue;

    private static IQueryable<BookingEntity> ApplyPassengerFilters(
        IQueryable<BookingEntity> query,
        DateTime? begin, DateTime? end, Gender? gender)
    {
        if (begin.HasValue) query = query.Where(b => b.Passenger.DateOfBirth >= DateOnly.FromDateTime(begin.Value));
        if (end.HasValue) query = query.Where(b => b.Passenger.DateOfBirth <= DateOnly.FromDateTime(end.Value));
        if (gender.HasValue) query = query.Where(b => b.Passenger.Gender == gender.Value);
        return query;
    }
}