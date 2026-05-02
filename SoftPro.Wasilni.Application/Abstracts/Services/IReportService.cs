using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IReportService
{
    Task<List<RidershipReportItem>> GetFromBookingsAsync(BookingReportFilterModel filter, CancellationToken cancellationToken);
    Task<List<RidershipReportItem>> GetFromRidershipAsync(RidershipReportFilterModel filter, CancellationToken cancellationToken);
}