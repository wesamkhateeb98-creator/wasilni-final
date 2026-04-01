using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IReportService
{
    Task<List<RidershipReportItem>> GetAsync(ReportType type, DateTime from, DateTime to, int? lineId, int? busId, CancellationToken cancellationToken);
}
