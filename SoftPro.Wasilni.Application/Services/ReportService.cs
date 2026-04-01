using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public Task<List<RidershipReportItem>> GetAsync(
        ReportType type, DateTime from, DateTime to, int? lineId, int? busId, CancellationToken cancellationToken)
        => type switch
        {
            ReportType.Daily   => GetDailyAsync(from, to, lineId, busId, cancellationToken),
            ReportType.Monthly => GetMonthlyAsync(from, to, lineId, busId, cancellationToken),
            ReportType.Yearly  => GetYearlyAsync(from, to, lineId, busId, cancellationToken),
            _                  => Task.FromResult(new List<RidershipReportItem>())
        };

    private async Task<List<RidershipReportItem>> GetDailyAsync(
        DateTime from, DateTime to, int? lineId, int? busId, CancellationToken ct)
    {
        var entities = await unitOfWork.DailyRidershipRepository
            .GetDailyAsync(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to), lineId, busId, ct);

        return entities
            .Select(e => new RidershipReportItem(e.LineId, e.BusId, e.Day.Year, e.Day.Month, e.Day, e.NumberOfRiders))
            .ToList();
    }

    private async Task<List<RidershipReportItem>> GetMonthlyAsync(
        DateTime from, DateTime to, int? lineId, int? busId, CancellationToken ct)
    {
        var results = await unitOfWork.DailyRidershipRepository
            .GetMonthlyAsync(from.Year, from.Month, to.Year, to.Month, lineId, busId, ct);

        return results
            .Select(r => new RidershipReportItem(lineId, busId, r.Year, r.Month, null, r.TotalRiders))
            .ToList();
    }

    private async Task<List<RidershipReportItem>> GetYearlyAsync(
        DateTime from, DateTime to, int? lineId, int? busId, CancellationToken ct)
    {
        var results = await unitOfWork.DailyRidershipRepository
            .GetYearlyAsync(from.Year, to.Year, lineId, busId, ct);

        return results
            .Select(r => new RidershipReportItem(lineId, busId, r.Year, null, null, r.TotalRiders))
            .ToList();
    }
}
