using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Hubs.Helpers;

/// <summary>
/// Strongly-typed wrappers around SignalR SendAsync calls.
/// Keeps magic strings out of the Hub and gives a single place
/// to rename or change the shape of any event payload.
/// </summary>
public static class TrackingHubEvents
{
    // ─── Event names ──────────────────────────────────────────────────────────
    private const string TripStarted           = "OnTripStarted";
    private const string TripEnded             = "OnTripEnded";
    private const string LocationUpdated       = "OnLocationUpdated";
    private const string AnonymousCountUpdated = "OnAnonymousCountUpdated";
    private const string BookingAdded          = "OnBookingAdded";
    private const string BookingCancelled      = "OnBookingCancelled";

    // ─── OnTripStarted ────────────────────────────────────────────────────────
    public static Task OnTripStartedAsync(
        this IClientProxy clients,
        GetTripResponse    payload,
        CancellationToken  ct = default)
        => clients.SendAsync(TripStarted, payload, ct);

    // ─── OnTripEnded ──────────────────────────────────────────────────────────
    public static Task OnTripEndedAsync(
        this IClientProxy clients,
        int               tripId,
        CancellationToken ct = default)
        => clients.SendAsync(TripEnded, new { tripId }, ct);

    // ─── OnLocationUpdated ────────────────────────────────────────────────────
    public static Task OnLocationUpdatedAsync(
        this IClientProxy clients,
        int               tripId,
        double            latitude,
        double            longitude,
        CancellationToken ct = default)
        => clients.SendAsync(LocationUpdated,
               new { tripId, latitude, longitude, updatedAt = DateTime.UtcNow }, ct);

    // ─── OnAnonymousCountUpdated ──────────────────────────────────────────────
    public static Task OnAnonymousCountUpdatedAsync(
        this IClientProxy clients,
        int               tripId,
        int               count,
        CancellationToken ct = default)
        => clients.SendAsync(AnonymousCountUpdated, new { tripId, count }, ct);

    // ─── OnBookingAdded (driver receives) ─────────────────────────────────────
    public static Task OnBookingAddedAsync(
        this IClientProxy   clients,
        GetBookingResponse  booking,
        CancellationToken   ct = default)
        => clients.SendAsync(BookingAdded, booking, ct);

    // ─── OnBookingCancelled (driver receives) ─────────────────────────────────
    public static Task OnBookingCancelledAsync(
        this IClientProxy clients,
        int               bookingId,
        CancellationToken ct = default)
        => clients.SendAsync(BookingCancelled, new { bookingId }, ct);
}
