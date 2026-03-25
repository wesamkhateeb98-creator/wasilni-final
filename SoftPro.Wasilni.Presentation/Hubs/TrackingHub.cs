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

    public async Task StartTrip(int busId, CancellationToken cancellationToken)
    {
        int driverId = Context.User!.GetId();
        GetTripModel trip = await tripService.StartTripAsync(busId, driverId, cancellationToken);

        await Groups.AddToGroupAsync(Context.ConnectionId, TripGroup(trip.Id), cancellationToken);

        var payload = trip.ToResponse();
        await Clients.Group(LineGroup(trip.LineId)).SendAsync("OnTripStarted", payload, cancellationToken);
        await Clients.Group("admin").SendAsync("OnTripStarted", payload, cancellationToken);
    }

    public async Task EndTrip(int tripId, CancellationToken cancellationToken)
    {
        int driverId = Context.User!.GetId();
        await tripService.EndTripAsync(tripId, driverId, cancellationToken);

        await Clients.Group(TripGroup(tripId)).SendAsync("OnTripEnded", new { tripId }, cancellationToken);
        await Clients.Group("admin").SendAsync("OnTripEnded", new { tripId }, cancellationToken);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, TripGroup(tripId), cancellationToken);
    }

    public async Task UpdateLocation(int tripId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        int driverId = Context.User!.GetId();
        await tripService.UpdateLocationAsync(tripId, latitude, longitude, driverId, cancellationToken);

        var payload = new { tripId, latitude, longitude, updatedAt = DateTime.UtcNow };
        await Clients.Group(TripGroup(tripId)).SendAsync("OnLocationUpdated", payload, cancellationToken);
        await Clients.Group("admin").SendAsync("OnLocationUpdated", payload, cancellationToken);
    }

    public async Task AdjustAnonymousPassenger(int tripId, int delta, CancellationToken cancellationToken)
    {
        int driverId = Context.User!.GetId();
        int newCount = await tripService.AdjustAnonymousAsync(tripId, delta, driverId, cancellationToken);

        await Clients.Group(TripGroup(tripId)).SendAsync("OnAnonymousCountUpdated",
            new { tripId, count = newCount }, cancellationToken);
    }

    // ─── Group Helpers ────────────────────────────────────────────────────────

    private static string TripGroup(int tripId) => $"trip-{tripId}";
    private static string LineGroup(int lineId) => $"line-{lineId}";
}
