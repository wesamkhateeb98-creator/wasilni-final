using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Presentation.Models.Response.Bus;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Hubs.Helpers;

/// <summary>
/// Strongly-typed wrappers around SignalR SendAsync calls.
/// Single place to rename events or change payload shapes.
/// </summary>
public static class TrackingHubEvents
{
    // ─── Event names ──────────────────────────────────────────────────────────
    private const string BusActivated = "OnBusActivated";
    private const string BusDeactivated = "OnBusDeactivated";
    private const string LocationUpdated = "OnLocationUpdated";
    private const string AnonymousCountUpdated = "OnAnonymousCountUpdated";
    private const string BookingAdded = "OnBookingAdded";
    private const string BookingCancelled = "OnBookingCancelled";
    private const string BookingStatusChanged = "OnBookingStatusChanged";

    // ─── OnBusActivated ───────────────────────────────────────────────────────
    public static Task OnBusActivatedAsync(
        this IClientProxy clients,
        GetActiveBusResponse payload,
        CancellationToken ct = default)
        => clients.SendAsync(BusActivated, payload, ct);

    // ─── OnBusDeactivated ─────────────────────────────────────────────────────
    public static Task OnBusDeactivatedAsync(
        this IClientProxy clients,
        int busId,
        CancellationToken ct = default)
        => clients.SendAsync(BusDeactivated, new { busId }, ct);

    // ─── OnLocationUpdated ────────────────────────────────────────────────────
    public static Task OnLocationUpdatedAsync(
        this IClientProxy clients,
        int busId,
        double latitude,
        double longitude,
        CancellationToken ct = default)
        => clients.SendAsync(LocationUpdated,
               new { busId, latitude, longitude, updatedAt = DateTime.UtcNow }, ct);

    // ─── OnAnonymousCountUpdated ──────────────────────────────────────────────
    public static Task OnAnonymousCountUpdatedAsync(
        this IClientProxy clients,
        int busId,
        int count,
        CancellationToken ct = default)
        => clients.SendAsync(AnonymousCountUpdated, new { busId, count }, ct);

    // ─── OnBookingAdded (driver receives) ─────────────────────────────────────
    public static Task OnBookingAddedAsync(
        this IClientProxy clients,
        GetBookingResponse booking,
        CancellationToken ct = default)
        => clients.SendAsync(BookingAdded, booking, ct);

    // ─── OnBookingStatusChanged (all drivers on line — confirmed / no-show) ───
    /// <summary>
    /// Broadcast to all drivers on the same line whenever a booking is
    /// confirmed (<c>PickedUp</c>) or marked as no-show (<c>Cancelled</c>).
    /// Clients should remove that booking from their local list.
    /// </summary>
    public static Task OnBookingStatusChangedAsync(
        this IClientProxy clients,
        int bookingId,
        string status,
        CancellationToken ct = default)
        => clients.SendAsync(BookingStatusChanged, new { bookingId, status }, ct);
}
