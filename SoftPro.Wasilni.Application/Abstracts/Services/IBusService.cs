using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;


namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBusService
{
    Task<Page<GetBusesModel>> GetBusesAsync(GetBusModel inputModel, CancellationToken cancellationToken);
    Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken);
    Task<int> RegisterAsync(RegisterBusModel registerModel, CancellationToken cancellationToken);
    Task<int> UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken);
    Task<int> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<GetBusesForAdminModel> AddDriver(AddDriverOnBusModel model, CancellationToken cancellationToken);
    Task<GetBusesForAdminModel> DeleteDriver(DeleteDriverFromBusModel model, CancellationToken cancellationToken);
    
}
