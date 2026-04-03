using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IReportService
{
    Task<List<RidershipReportItem>> GetAsync(GetReportFilterModel filter, CancellationToken cancellationToken);
}
