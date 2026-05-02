// Infrastructure/Repositories/DailyRidershipRepository.cs
using Microsoft.EntityFrameworkCore;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;
using SoftPro.Wasilni.Infrastructure.Persistence;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class DailyRidershipRepository(AppDbContext dbContext)
    : Repository<DailyRidershipEntity>(dbContext), IDailyRidershipRepository
{
    public Task<DailyRidershipEntity?> GetByLineIdAndBusIdAsync(IncrementRidershipModel model, CancellationToken cancellationToken)
        => dbContext.DailyRiderships
            .FirstOrDefaultAsync(
                r => r.LineId == model.LineId && r.BusId == model.BusId && r.Day == model.Day,
                cancellationToken);

    // ═══════════════════════════════════════════════════════════════════
    // Bookings Source — supports passenger filters
    // ═══════════════════════════════════════════════════════════════════

    public async Task<List<RidershipReportItem>> GetFromBookingsAsync(
        BookingReportFilterModel filter, CancellationToken cancellationToken)
    {
        var from = DateOnly.FromDateTime(filter.From);
        var to = DateOnly.FromDateTime(filter.To);

        var query = dbContext.Bookings
            .Where(b => b.Date >= from && b.Date <= to);

        if (filter.LineId.HasValue)
            query = query.Where(b => b.LineId == filter.LineId.Value);
        if (filter.BeginDateOfBirth.HasValue)
            query = query.Where(b => b.Passenger.DateOfBirth >= DateOnly.FromDateTime(filter.BeginDateOfBirth.Value));
        if (filter.EndDateOfBirth.HasValue)
            query = query.Where(b => b.Passenger.DateOfBirth < DateOnly.FromDateTime(filter.EndDateOfBirth.Value));
        if (filter.Gender.HasValue)
            query = query.Where(b => b.Passenger.Gender == filter.Gender.Value);
        if (filter.Status.HasValue)
            query = query.Where(b => b.Status == filter.Status.Value);

        return filter.Type switch
        {
            ReportType.Daily => await GetDailyFromBookingsAsync(query, filter.LineId, cancellationToken),
            ReportType.Monthly => await GetMonthlyFromBookingsAsync(query, filter.LineId, cancellationToken),
            ReportType.Yearly => await GetYearlyFromBookingsAsync(query, filter.LineId, cancellationToken),
            _ => []
        };
    }

    private static async Task<List<RidershipReportItem>> GetDailyFromBookingsAsync(
        IQueryable<BookingEntity> query, int? lineId, CancellationToken ct)
    {
        var rows = await query
            .GroupBy(b => new { b.Date, b.LineId })
            .OrderBy(g => g.Key.Date)
            .Select(g => new { g.Key.LineId, g.Key.Date, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .Select(r => new RidershipReportItem(r.LineId, null, r.Date.Year, r.Date.Month, r.Date, r.Count))
            .ToList();
    }

    private static async Task<List<RidershipReportItem>> GetMonthlyFromBookingsAsync(
        IQueryable<BookingEntity> query, int? lineId, CancellationToken ct)
    {
        var rows = await query
            .GroupBy(b => new { b.Date.Year, b.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .Select(r => new RidershipReportItem(lineId, null, r.Year, r.Month, null, r.Count))
            .ToList();
    }

    private static async Task<List<RidershipReportItem>> GetYearlyFromBookingsAsync(
        IQueryable<BookingEntity> query, int? lineId, CancellationToken ct)
    {
        var rows = await query
            .GroupBy(b => b.Date.Year)
            .OrderBy(g => g.Key)
            .Select(g => new { Year = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return rows
            .Select(r => new RidershipReportItem(lineId, null, r.Year, null, null, r.Count))
            .ToList();
    }

    // ═══════════════════════════════════════════════════════════════════
    // DailyRidership Source — fast aggregated counter
    // ═══════════════════════════════════════════════════════════════════

    public async Task<List<RidershipReportItem>> GetFromRidershipAsync(
        RidershipReportFilterModel filter, CancellationToken cancellationToken)
    {
        var from = DateOnly.FromDateTime(filter.From);
        var to = DateOnly.FromDateTime(filter.To);

        var query = dbContext.DailyRiderships
            .Where(r => r.Day >= from && r.Day <= to);

        if (filter.LineId.HasValue)
            query = query.Where(r => r.LineId == filter.LineId.Value);
        if (filter.BusId.HasValue)
            query = query.Where(r => r.BusId == filter.BusId.Value);

        return filter.Type switch
        {
            ReportType.Daily => await GetDailyFromRidershipAsync(query, filter, cancellationToken),
            ReportType.Monthly => await GetMonthlyFromRidershipAsync(query, filter, cancellationToken),
            ReportType.Yearly => await GetYearlyFromRidershipAsync(query, filter, cancellationToken),
            _ => []
        };
    }

    private static async Task<List<RidershipReportItem>> GetDailyFromRidershipAsync(
        IQueryable<DailyRidershipEntity> query, RidershipReportFilterModel filter, CancellationToken ct)
    {
        // لاين بدون باص: نجمع كل الباصات في نفس اليوم
        if (filter.LineId.HasValue && !filter.BusId.HasValue)
        {
            var grouped = await query
                .GroupBy(r => r.Day)
                .OrderBy(g => g.Key)
                .Select(g => new { Day = g.Key, Total = g.Sum(r => r.NumberOfRiders) })
                .ToListAsync(ct);

            return grouped
                .Select(r => new RidershipReportItem(filter.LineId, null, r.Day.Year, r.Day.Month, r.Day, r.Total))
                .ToList();
        }

        var rows = await query.OrderBy(r => r.Day).ToListAsync(ct);

        return rows
            .Select(r => new RidershipReportItem(r.LineId, r.BusId, r.Day.Year, r.Day.Month, r.Day, r.NumberOfRiders))
            .ToList();
    }

    private static async Task<List<RidershipReportItem>> GetMonthlyFromRidershipAsync(
        IQueryable<DailyRidershipEntity> query, RidershipReportFilterModel filter, CancellationToken ct)
    {
        var rows = await query
            .GroupBy(r => new { r.Day.Year, r.Day.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(r => r.NumberOfRiders) })
            .ToListAsync(ct);

        return rows
            .Select(r => new RidershipReportItem(filter.LineId, filter.BusId, r.Year, r.Month, null, r.Total))
            .ToList();
    }

    private static async Task<List<RidershipReportItem>> GetYearlyFromRidershipAsync(
        IQueryable<DailyRidershipEntity> query, RidershipReportFilterModel filter, CancellationToken ct)
    {
        var rows = await query
            .GroupBy(r => r.Day.Year)
            .OrderBy(g => g.Key)
            .Select(g => new { Year = g.Key, Total = g.Sum(r => r.NumberOfRiders) })
            .ToListAsync(ct);

        return rows
            .Select(r => new RidershipReportItem(filter.LineId, filter.BusId, r.Year, null, null, r.Total))
            .ToList();
    }
}