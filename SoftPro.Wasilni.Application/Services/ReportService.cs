using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public Task<List<RidershipReportItem>> GetAsync(GetReportFilterModel filter, CancellationToken cancellationToken)
        => filter.Type switch
        {
            ReportType.Daily   => GetDailyAsync(filter, cancellationToken),
            ReportType.Monthly => GetMonthlyAsync(filter, cancellationToken),
            ReportType.Yearly  => GetYearlyAsync(filter, cancellationToken),
            _                  => Task.FromResult(new List<RidershipReportItem>())
        };

    private async Task<List<RidershipReportItem>> GetDailyAsync(GetReportFilterModel filter, CancellationToken ct)
    {
        var dailyFilter = new GetDailyFilterModel(
        DateOnly.FromDateTime(filter.From),
        DateOnly.FromDateTime(filter.To),
        filter.LineId, filter.BusId,
        filter.BeginDateOfBirth, filter.EndDateOfBirth, filter.Gender, filter.Status);

        var entities = await unitOfWork.DailyRidershipRepository.GetDailyAsync(dailyFilter, ct);

        // Line report: sum all buses per day
        if (filter.LineId.HasValue && !filter.BusId.HasValue)
            return entities
                .GroupBy(e => e.Day)
                .OrderBy(g => g.Key)
                .Select(g => new RidershipReportItem(filter.LineId, null, g.Key.Year, g.Key.Month, g.Key, g.Sum(e => e.NumberOfRiders)))
                .ToList();

        return entities
            .Select(e => new RidershipReportItem(e.LineId, e.BusId, e.Day.Year, e.Day.Month, e.Day, e.NumberOfRiders))
            .ToList();
    }

    private async Task<List<RidershipReportItem>> GetMonthlyAsync(GetReportFilterModel filter, CancellationToken ct)
    {
        var monthlyFilter = new GetMonthlyFilterModel(
         filter.From.Year, filter.From.Month,
         filter.To.Year, filter.To.Month,
         filter.LineId, filter.BusId,
         filter.BeginDateOfBirth, filter.EndDateOfBirth, filter.Gender, filter.Status);

        var results = await unitOfWork.DailyRidershipRepository.GetMonthlyAsync(monthlyFilter, ct);

        return results
            .Select(r => new RidershipReportItem(filter.LineId, filter.BusId, r.Year, r.Month, null, r.TotalRiders))
            .ToList();
    }

    private async Task<List<RidershipReportItem>> GetYearlyAsync(GetReportFilterModel filter, CancellationToken ct)
    {
        var yearlyFilter = new GetYearlyFilterModel(
       filter.From.Year, filter.To.Year,
       filter.LineId, filter.BusId,
       filter.BeginDateOfBirth, filter.EndDateOfBirth, filter.Gender, filter.Status);

        var results = await unitOfWork.DailyRidershipRepository.GetYearlyAsync(yearlyFilter, ct);

        return results
            .Select(r => new RidershipReportItem(filter.LineId, filter.BusId, r.Year, null, null, r.TotalRiders))
            .ToList();
    }


}
