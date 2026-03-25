using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;

namespace SoftPro.Wasilni.Presentation.Hubs;

[Authorize]
public class TrackingHub(ITripService tripService) : Hub
{
    // ─── Driver Methods ───────────────────────────────────────────────────────

    public async Task StartTrip(int busId)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        GetTripModel trip = await tripService.StartTripAsync(busId, driverId, ct);

        await Groups.AddToGroupAsync(Context.ConnectionId, TripGroup(trip.Id), ct);

        var payload = trip.ToResponse();
        await Clients.Group(LineGroup(trip.LineId)).SendAsync("OnTripStarted", payload, ct);
        await Clients.Group("admin").SendAsync("OnTripStarted", payload, ct);

        // Also notify the driver themselves
        await Clients.Caller.SendAsync("OnTripStarted", payload, ct);
    }

    public async Task EndTrip(int tripId)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await tripService.EndTripAsync(tripId, driverId, ct);

        var payload = new { tripId };
        await Clients.Group(TripGroup(tripId)).SendAsync("OnTripEnded", payload, ct);
        await Clients.Group("admin").SendAsync("OnTripEnded", payload, ct);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TripGroup(tripId), ct);
    }

    public async Task UpdateLocation(int tripId, double latitude, double longitude)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        await tripService.UpdateLocationAsync(tripId, latitude, longitude, driverId, ct);

        var payload = new { tripId, latitude, longitude, updatedAt = DateTime.UtcNow };
        await Clients.Group(TripGroup(tripId)).SendAsync("OnLocationUpdated", payload, ct);
        await Clients.Group("admin").SendAsync("OnLocationUpdated", payload, ct);
    }

    public async Task AdjustAnonymousPassenger(int tripId, int delta)
    {
        var ct       = Context.ConnectionAborted;
        int driverId = Context.User!.GetId();

        int newCount = await tripService.AdjustAnonymousAsync(tripId, delta, driverId, ct);

        await Clients.Group(TripGroup(tripId)).SendAsync("OnAnonymousCountUpdated",
            new { tripId, count = newCount }, ct);
    }

    // ─── Group Helpers ────────────────────────────────────────────────────────

    private static string TripGroup(int tripId) => $"trip-{tripId}";
    private static string LineGroup(int lineId) => $"line-{lineId}";
}
