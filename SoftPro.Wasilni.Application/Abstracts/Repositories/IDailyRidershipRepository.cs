using SoftPro.Wasilni.Domain.Entities;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IDailyRidershipRepository : IRepository<DailyRidershipEntity>
{
    /// <summary>Increments NumberOfRiders atomically with RowVersion retry. Returns new count.</summary>
    Task<int> IncrementAsync(int busId, DateOnly day, CancellationToken cancellationToken);
}
