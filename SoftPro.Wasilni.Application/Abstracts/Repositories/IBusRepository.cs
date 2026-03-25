using SoftPro.Wasilni.Domain.Entities;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;


namespace SoftPro.Wasilni.Application.Abstracts.Repositories;

public interface IBusRepository : IRepository<BusEntity>
{
    Task<bool> ExistsPlateAsync(string plate, CancellationToken cancellationToken);
    Task<bool> ExistsPlateExceptAsync(string plate, int excludeId, CancellationToken cancellationToken);
    Task<Page<GetBusesModel>> GetAllBusesForPassengerIdAsync(GetBusModel inputModel, CancellationToken cancellationToken);
    Task<Page<GetBusesForAdminModel>> GetAllBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken);
    Task<BusEntity?> GetWithRequestByBusIdAsync(int busId, CancellationToken cancellationToken);
    Task<BusEntity?> GetWithBusWithRequestBusByIdAsync(int busId, CancellationToken cancellationToken);
    Task<BusEntity?> GetWithRequestByIdAsync(int id, CancellationToken cancellationToken);
    Task<BusEntity?> GetByIdWithDriverAsync(int busId, CancellationToken cancellationToken);
    Task<BusEntity?> GetByIdWithLineAsync(int id, CancellationToken cancellationToken);
}
