using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;

namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IBusRepository : IRepository<BusEntity>
{
    Task<bool> ExistsPlateAsync(string plate, CancellationToken cancellationToken);
    Task<bool> ExistsPlateExceptAsync(string plate, int excludeId, CancellationToken cancellationToken);
    Task<Page<GetBusesForAdminModel>> GetAllBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken);
    Task<BusEntity?> GetByIdWithDriverAsync(int busId, CancellationToken cancellationToken);
    Task<BusEntity?> GetByDriverIdAsync(int driverId, CancellationToken cancellationToken);
    Task<BusEntity?> FindByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);
    Task<bool> HasActiveBusOnLineAsync(int lineId, CancellationToken cancellationToken);
}
