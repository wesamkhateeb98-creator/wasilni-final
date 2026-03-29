using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public Task<List<RidershipReportItem>> GetAsync(
        ReportType type, DateTime from, DateTime to, int? lineId, CancellationToken cancellationToken)
        => type switch
        {
            ReportType.Daily   => GetDailyAsync(from, to, lineId, cancellationToken),
            ReportType.Monthly => GetMonthlyAsync(from, to, lineId, cancellationToken),
            ReportType.Yearly  => GetYearlyAsync(from, to, lineId, cancellationToken),
            _                  => Task.FromResult(new List<RidershipReportItem>())
        };

    private async Task<List<RidershipReportItem>> GetDailyAsync(
        DateTime from, DateTime to, int? lineId, CancellationToken ct)
    {
        var entities = await unitOfWork.DailyRidershipRepository.GetDailyAsync(
            DateOnly.FromDateTime(from), DateOnly.FromDateTime(to), lineId, ct);

        return entities
            .Select(e => new RidershipReportItem(null, e.LineId, e.Day.Year, e.Day.Month, e.Day, e.NumberOfRiders))
            .ToList();
    }

    private async Task<List<RidershipReportItem>> GetMonthlyAsync(
        DateTime from, DateTime to, int? lineId, CancellationToken ct)
    {
        var results = await unitOfWork.DailyRidershipRepository.GetMonthlyAsync(
            from.Year, from.Month, to.Year, to.Month, lineId, ct);

        return results
            .Select(r => new RidershipReportItem(null, lineId, r.Year, r.Month, null, r.TotalRiders))
            .ToList();
    }

    private async Task<List<RidershipReportItem>> GetYearlyAsync(
        DateTime from, DateTime to, int? lineId, CancellationToken ct)
    {
        var results = await unitOfWork.DailyRidershipRepository.GetYearlyAsync(
            from.Year, to.Year, lineId, ct);

        return results
            .Select(r => new RidershipReportItem(null, lineId, r.Year, null, null, r.TotalRiders))
            .ToList();
    }
}
