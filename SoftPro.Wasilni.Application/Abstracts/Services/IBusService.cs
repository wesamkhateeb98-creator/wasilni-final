using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBusService
{
    // ─── Admin CRUD ───────────────────────────────────────────────────────────
    Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken);
    Task<int> AddAsync(AddBusModel model, CancellationToken cancellationToken);
    Task<int> UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken);
    Task<int> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<int> AddDriverAsync(int busId, int driverId, CancellationToken cancellationToken);
    Task<int> DeleteDriverAsync(int busId, CancellationToken cancellationToken);

    // ─── Driver: Bus state ────────────────────────────────────────────────────
    Task<DriverBusInfoModel> GetBusInfoAsync(int driverId, CancellationToken cancellationToken);
    Task<GetActiveBusModel> ActivateBusAsync(int driverId, CancellationToken cancellationToken);
    Task<GetActiveBusModel> DeactivateBusAsync(int driverId, CancellationToken cancellationToken);
    Task<UpdateLocationResult> UpdateLocationAsync(UpdateBusLocationModel model, CancellationToken cancellationToken);
    Task<AdjustAnonymousResult> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken);
}
