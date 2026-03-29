using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.ActionFilters.Authorization;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Hubs;
using SoftPro.Wasilni.Presentation.Hubs.Helpers;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Trip;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/bookings")]
public class BookingsController(
    IBusService busService,
    IHubContext<TrackingHub> hubContext) : BaseController
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DRIVER endpoints
    // ═══════════════════════════════════════════════════════════════════════════

    [HttpGet("nearby")]
    [Authorize]
    [HasBus]
    public async Task<List<GetBookingResponse>> GetNearbyBookingsAsync(
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        List<GetBookingModel> models =
            await busService.GetNearbyBookingsAsync(driverId, cancellationToken);

        return models.Select(m => m.ToResponse()).ToList();
    }

    [HttpPut("{id}/confirm")]
    [Authorize]
    [HasBus]
    public async Task<IdResponse> ConfirmBookingAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        var (bookingId, lineId) =
            await busService.ConfirmBookingAsync(id, driverId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.Line(lineId))
            .OnBookingStatusChangedAsync(bookingId, BookingStatus.PickedUp.ToString(), cancellationToken);

        return new IdResponse(bookingId);
    }

    [HttpPut("{id}/no-show")]
    [Authorize]
    [HasBus]
    public async Task<IdResponse> MarkNoShowAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        var (bookingId, lineId) =
            await busService.MarkNoShowAsync(id, driverId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.Line(lineId))
            .OnBookingStatusChangedAsync(bookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new IdResponse(bookingId);
    }

    // ─── Passenger: Bookings ──────────────────────────────────────────────────

    [HttpPost("{id}/bookings")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<IdResponse> AddBookingAsync(
        [FromRoute] IdRequest route,
        [FromBody] AddBookingRequest request,
        CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        int bookingId = await busService.AddBookingAsync(
            route.Id, passengerId, request.Latitude, request.Longitude, cancellationToken);

        // Notify all active drivers on this line
        var notification = new GetBookingResponse(
            bookingId, route.Id, passengerId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            request.Latitude, request.Longitude,
            BookingStatus.Waiting.ToString(),
            DateTime.UtcNow);

        await hubContext.Clients
            .Group(TrackingGroups.Line(route.Id))
            .OnBookingAddedAsync(notification, cancellationToken);

        return new(bookingId);
    }

    [HttpDelete("{id}/bookings")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<IdResponse> CancelBookingAsync(
        [FromRoute] IdRequest route,
        CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        int bookingId = await busService.CancelBookingAsync(route.Id, passengerId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.Line(route.Id))
            .OnBookingStatusChangedAsync(bookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new(bookingId);
    }
}
