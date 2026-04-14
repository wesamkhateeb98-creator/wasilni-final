using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models.Reports;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IDailyRidershipRepository : IRepository<DailyRidershipEntity>
{
    Task<int> IncrementAsync(IncrementRidershipModel model, CancellationToken cancellationToken);
    Task<DailyRidershipEntity?> GetOrCreateAsync(IncrementRidershipModel model, CancellationToken cancellationToken);

    Task<List<DailyRidershipEntity>> GetDailyAsync(GetDailyFilterModel filter, CancellationToken cancellationToken);

    Task<List<MonthlyRidershipResult>> GetMonthlyAsync(GetMonthlyFilterModel filter, CancellationToken cancellationToken);

    Task<List<YearlyRidershipResult>> GetYearlyAsync(GetYearlyFilterModel filter, CancellationToken cancellationToken);
}
