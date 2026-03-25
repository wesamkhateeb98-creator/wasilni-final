using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Hubs.Helpers;
using SoftPro.Wasilni.Presentation.Models.Request.Hub;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Hubs;

[Authorize]
public class TrackingHub(ITripService tripService) : Hub
{
    // ─── Driver: Start Trip ───────────────────────────────────────────────────

    public async Task StartTrip(StartTripHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        GetTripModel    trip    = await tripService.StartTripAsync(request.BusId, driverId, ct);
        GetTripResponse payload = trip.ToResponse();

        await Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Trip(trip.Id), ct);

        await Clients.Group(TrackingGroups.Line(trip.LineId)).OnTripStartedAsync(payload, ct);
        await Clients.Group(TrackingGroups.Admin).OnTripStartedAsync(payload, ct);
        await Clients.Caller.OnTripStartedAsync(payload, ct);
    }

    // ─── Driver: End Trip ─────────────────────────────────────────────────────

    public async Task EndTrip(EndTripHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await tripService.EndTripAsync(request.TripId, driverId, ct);

        await Clients.Group(TrackingGroups.Trip(request.TripId)).OnTripEndedAsync(request.TripId, ct);
        await Clients.Group(TrackingGroups.Admin).OnTripEndedAsync(request.TripId, ct);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.Trip(request.TripId), ct);
    }

    // ─── Driver: Update Location ──────────────────────────────────────────────

    public async Task UpdateLocation(UpdateLocationHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await tripService.UpdateLocationAsync(request.TripId, request.Latitude, request.Longitude, driverId, ct);

        await Clients.Group(TrackingGroups.Trip(request.TripId))
                     .OnLocationUpdatedAsync(request.TripId, request.Latitude, request.Longitude, ct);

        await Clients.Group(TrackingGroups.Admin)
                     .OnLocationUpdatedAsync(request.TripId, request.Latitude, request.Longitude, ct);
    }

    // ─── Driver: Adjust Anonymous Passengers ─────────────────────────────────

    public async Task AdjustAnonymousPassenger(AdjustAnonymousHubRequest request)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        int newCount = await tripService.AdjustAnonymousAsync(request.TripId, request.Delta, driverId, ct);

        await Clients.Group(TrackingGroups.Trip(request.TripId))
                     .OnAnonymousCountUpdatedAsync(request.TripId, newCount, ct);
    }

    // ─── Passenger: Subscribe / Unsubscribe ──────────────────────────────────

    public Task SubscribeToTrip(int tripId)
        => Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Trip(tripId), Context.ConnectionAborted);

    public Task SubscribeToLine(int lineId)
        => Groups.AddToGroupAsync(Context.ConnectionId, TrackingGroups.Line(lineId), Context.ConnectionAborted);

    public Task UnsubscribeFromTrip(int tripId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, TrackingGroups.Trip(tripId), Context.ConnectionAborted);
}
