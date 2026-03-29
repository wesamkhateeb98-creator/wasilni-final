using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IDailyRidershipRepository : IRepository<DailyRidershipEntity>
{
    /// <summary>Increments NumberOfRiders atomically with RowVersion retry. Returns new count.</summary>
    Task<int> IncrementAsync(int busId, int lineId, DateOnly day, CancellationToken cancellationToken);

    Task<List<DailyRidershipEntity>> GetDailyAsync(DateOnly from, DateOnly to, int? lineId, CancellationToken cancellationToken);

    Task<List<(int Year, int Month, int TotalRiders)>> GetMonthlyAsync(int fromYear, int fromMonth, int toYear, int toMonth, int? lineId, CancellationToken cancellationToken);

    Task<List<(int Year, int TotalRiders)>> GetYearlyAsync(int fromYear, int toYear, int? lineId, CancellationToken cancellationToken);
}
