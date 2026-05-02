using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IDailyRidershipRepository : IRepository<DailyRidershipEntity>
{
    Task<DailyRidershipEntity?> GetByLineIdAndBusIdAsync(IncrementRidershipModel model, CancellationToken cancellationToken);
    Task<List<RidershipReportItem>> GetFromBookingsAsync(BookingReportFilterModel filter, CancellationToken cancellationToken);
    Task<List<RidershipReportItem>> GetFromRidershipAsync(RidershipReportFilterModel filter, CancellationToken cancellationToken);
}