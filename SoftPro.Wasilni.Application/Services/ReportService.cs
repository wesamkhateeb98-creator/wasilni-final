// Application/Services/ReportService.cs
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Services;

public class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public Task<List<RidershipReportItem>> GetFromBookingsAsync(
        BookingReportFilterModel filter, CancellationToken cancellationToken)
        => unitOfWork.DailyRidershipRepository.GetFromBookingsAsync(filter, cancellationToken);

    public Task<List<RidershipReportItem>> GetFromRidershipAsync(
        RidershipReportFilterModel filter, CancellationToken cancellationToken)
        => unitOfWork.DailyRidershipRepository.GetFromRidershipAsync(filter, cancellationToken);
}