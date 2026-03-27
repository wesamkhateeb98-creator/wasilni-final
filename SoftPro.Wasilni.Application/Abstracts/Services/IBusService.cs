using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Domain.Models.Trips;

namespace SoftPro.Wasilni.Application.Abstracts.Services;

public interface IBusService
{
    // ─── Admin CRUD ───────────────────────────────────────────────────────────
    Task<Page<GetBusesModel>>         GetBusesAsync(GetBusModel inputModel, CancellationToken cancellationToken);
    Task<Page<GetBusesForAdminModel>> GetBusesForAdminAsync(GetBusForAdminModel inputModel, CancellationToken cancellationToken);
    Task<int>                RegisterAsync(RegisterBusModel registerModel, CancellationToken cancellationToken);
    Task<int>                UpdateAsync(int id, UpdateBusModel model, CancellationToken cancellationToken);
    Task<int>                DeleteAsync(int id, CancellationToken cancellationToken);
    Task<int> AddDriver(AddDriverOnBusModel model, CancellationToken cancellationToken);
    Task<int> DeleteDriver(DeleteDriverFromBusModel model, CancellationToken cancellationToken);

    // ─── Driver Tracking ──────────────────────────────────────────────────────
    Task<GetActiveBusModel>  ToggleStatusAsync(int driverId, CancellationToken cancellationToken);
    Task<int>                UpdateLocationAsync(int driverId, double latitude, double longitude, CancellationToken cancellationToken);
    Task<(int BusId, int Count)> AdjustAnonymousAsync(int driverId, int delta, CancellationToken cancellationToken);
    Task<int>                ConfirmRiderAsync(int driverId, CancellationToken cancellationToken);
    Task<GetActiveBusModel?> GetMyActiveBusAsync(int driverId, CancellationToken cancellationToken);

    // ─── Passenger ────────────────────────────────────────────────────────────
    Task<List<GetActiveBusModel>> GetActiveBusesAsync(int? lineId, CancellationToken cancellationToken);
    Task<GetBookingModel>    AddBookingAsync(int lineId, int passengerId, double latitude, double longitude, CancellationToken cancellationToken);
    Task<int>                CancelBookingAsync(int lineId, int passengerId, CancellationToken cancellationToken);
}
