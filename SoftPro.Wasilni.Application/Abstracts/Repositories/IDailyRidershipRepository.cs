using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IDailyRidershipRepository : IRepository<DailyRidershipEntity>
{
    Task<int> IncrementAsync(int lineId, int busId, DateOnly day, CancellationToken cancellationToken);

    Task<List<DailyRidershipEntity>> GetDailyAsync(DateOnly from, DateOnly to, int? lineId, int? busId, CancellationToken cancellationToken);

    Task<List<(int Year, int Month, int TotalRiders)>> GetMonthlyAsync(int fromYear, int fromMonth, int toYear, int toMonth, int? lineId, int? busId, CancellationToken cancellationToken);

    Task<List<(int Year, int TotalRiders)>> GetYearlyAsync(int fromYear, int toYear, int? lineId, int? busId, CancellationToken cancellationToken);
}
