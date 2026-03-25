using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;

namespace SoftPro.Wasilni.Presentation.Hubs;

[Authorize]
public class TrackingHub(ITripService tripService) : Hub
{
    // ─── Driver Methods ───────────────────────────────────────────────────────

    public async Task StartTrip(StartTripHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        GetTripModel trip = await tripService.StartTripAsync(request.BusId, driverId, ct);

        await Groups.AddToGroupAsync(Context.ConnectionId, TripGroup(trip.Id), ct);

        var payload = trip.ToResponse();
        await Clients.Group(LineGroup(trip.LineId)).SendAsync("OnTripStarted", payload, ct);
        await Clients.Group("admin").SendAsync("OnTripStarted", payload, ct);
        await Clients.Caller.SendAsync("OnTripStarted", payload, ct);
    }

    public async Task EndTrip(EndTripHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await tripService.EndTripAsync(request.TripId, driverId, ct);

        var payload = new { tripId = request.TripId };
        await Clients.Group(TripGroup(request.TripId)).SendAsync("OnTripEnded", payload, ct);
        await Clients.Group("admin").SendAsync("OnTripEnded", payload, ct);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TripGroup(request.TripId), ct);
    }

    public async Task UpdateLocation(UpdateLocationHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await tripService.UpdateLocationAsync(request.TripId, request.Latitude, request.Longitude, driverId, ct);

        var payload = new
        {
            tripId    = request.TripId,
            latitude  = request.Latitude,
            longitude = request.Longitude,
            updatedAt = DateTime.UtcNow
        };
        await Clients.Group(TripGroup(request.TripId)).SendAsync("OnLocationUpdated", payload, ct);
        await Clients.Group("admin").SendAsync("OnLocationUpdated", payload, ct);
    }

    public async Task AdjustAnonymousPassenger(AdjustAnonymousHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        int newCount = await tripService.AdjustAnonymousAsync(request.TripId, request.Delta, driverId, ct);

        await Clients.Group(TripGroup(request.TripId)).SendAsync("OnAnonymousCountUpdated",
            new { tripId = request.TripId, count = newCount }, ct);
    }

    // ─── Passenger Methods ────────────────────────────────────────────────────

    public async Task SubscribeToTrip(int tripId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, TripGroup(tripId), Context.ConnectionAborted);
    }

    public async Task SubscribeToLine(int lineId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, LineGroup(lineId), Context.ConnectionAborted);
    }

    public async Task UnsubscribeFromTrip(int tripId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TripGroup(tripId), Context.ConnectionAborted);
    }

    // ─── Group Helpers ────────────────────────────────────────────────────────

    private static string TripGroup(int tripId) => $"trip-{tripId}";
    private static string LineGroup(int lineId) => $"line-{lineId}";
}
