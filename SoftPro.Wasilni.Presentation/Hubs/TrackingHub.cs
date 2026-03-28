using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Presentation.ActionFilters.Authorization;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Hubs.Helpers;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Hubs;

[Authorize]
public class TrackingHub(IBusService busService, IMemoryCache cache) : Hub
{
    // ─── Reconnect: restore line group membership ────────────────────────────

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        if (int.TryParse(
                Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                out int userId) &&
            cache.TryGetValue(BusCacheKeys.DriverLine(userId), out int lineId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(lineId), Context.ConnectionAborted);
        }
    }

    // ─── Driver: Toggle bus on/off route ─────────────────────────────────────

    [HasBus]
    public async Task ToggleStatus()
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        GetActiveBusModel result = await busService.ToggleStatusAsync(driverId, ct);
        var response = result.ToResponse();

        if (result.Status == BusStatus.Active)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(result.LineId), ct);

            await Clients.Group(TrackingGroups.Line(result.LineId)).OnBusActivatedAsync(response, ct);
            await Clients.Group(TrackingGroups.Admin).OnBusActivatedAsync(response, ct);
            await Clients.Caller.OnBusActivatedAsync(response, ct);
        }
        else
        {
            await Clients.Group(TrackingGroups.Line(result.LineId)).OnBusDeactivatedAsync(result.BusId, ct);
            await Clients.Group(TrackingGroups.Admin).OnBusDeactivatedAsync(result.BusId, ct);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.Line(result.LineId), ct);
        }
    }

    // ─── Driver: Send GPS location ────────────────────────────────────────────

    [HasBus]
    public async Task UpdateLocation(UpdateLocationHubRequest request)
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        var (busId, lineId) = await busService.UpdateLocationAsync(driverId, request.Latitude, request.Longitude, ct);

        await Clients.Group(TrackingGroups.Line(lineId))
                     .OnLocationUpdatedAsync(busId, request.Latitude, request.Longitude, ct);

        await Clients.Group(TrackingGroups.Admin)
                     .OnLocationUpdatedAsync(busId, request.Latitude, request.Longitude, ct);
    }

    // ─── Driver: Adjust anonymous passenger count ─────────────────────────────

    [HasBus]
    public async Task AdjustAnonymousPassenger(AdjustAnonymousHubRequest request)
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        var (busId, lineId, count) = await busService.AdjustAnonymousAsync(driverId, request.Delta, ct);

        await Clients.Group(TrackingGroups.Line(lineId))
                     .OnAnonymousCountUpdatedAsync(busId, count, ct);

        await Clients.Group(TrackingGroups.Admin)
                     .OnAnonymousCountUpdatedAsync(busId, count, ct);
    }

    // ─── Driver: Confirm a passenger boarded ─────────────────────────────────

    [HasBus]
    public async Task ConfirmRider()
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await busService.ConfirmRiderAsync(driverId, ct);
    }

    // ─── Passenger / Admin: Subscribe to a line ──────────────────────────────

    public Task SubscribeToLine(int lineId)
        => Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(lineId), Context.ConnectionAborted);

    public Task UnsubscribeFromLine(int lineId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.Line(lineId), Context.ConnectionAborted);

    // ─── Admin: Join admin group ──────────────────────────────────────────────

    [Authorize(Roles = nameof(Role.Admin))]
    public Task JoinAdminGroup()
        => Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Admin, Context.ConnectionAborted);
}
