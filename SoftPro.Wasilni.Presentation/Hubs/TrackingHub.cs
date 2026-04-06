using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Application.Cache;
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
            cache.TryGetValue(BusCacheKeys.DriverContext(userId), out DriverContextCache? ctx) &&
            ctx is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(ctx.LineId), Context.ConnectionAborted);
            await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.LineBooking(ctx.LineId), Context.ConnectionAborted);
        }
    }

    // ─── Driver: Activate bus ─────────────────────────────────────────────────

    [HasBus]
    public async Task ActiveBus()
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        GetActiveBusModel result = await busService.ActivateBusAsync(driverId, ct);
        var response = result.ToResponse();

        await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(result.LineId), ct);
        await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.LineBooking(result.LineId), ct);

        await Clients.Group(TrackingGroups.Line(result.LineId)).OnBusActivatedAsync(response, ct);
        await Clients.Caller.OnBusActivatedAsync(response, ct);
    }

    // ─── Driver: Deactivate bus ───────────────────────────────────────────────

    [HasBus]
    public async Task InactiveBus()
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        GetActiveBusModel result = await busService.DeactivateBusAsync(driverId, ct);

        await Clients.Group(TrackingGroups.Line(result.LineId)).OnBusDeactivatedAsync(result.BusId, ct);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.Line(result.LineId), ct);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.LineBooking(result.LineId), ct);
    }

    // ─── Driver: Send GPS location ────────────────────────────────────────────

    [HasBus]
    public async Task UpdateLocation(UpdateLocationHubRequest request)
    {
        var ct = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        UpdateLocationResult result = await busService.UpdateLocationAsync(
            new UpdateBusLocationModel(driverId, request.Latitude, request.Longitude), ct);

        await Clients.Group(TrackingGroups.Line(result.LineId))
                     .OnLocationUpdatedAsync(result.BusId, request.Latitude, request.Longitude, ct);
    }

    // ─── Passenger / Admin: Subscribe to a line ──────────────────────────────

    public Task SubscribeToLine(int lineId)
        => Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(lineId), Context.ConnectionAborted);

    public Task UnsubscribeFromLine(int lineId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.Line(lineId), Context.ConnectionAborted);
}
