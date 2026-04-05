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

    [HttpGet("line")]
    [Authorize]
    [HasBus]
    public async Task<List<GetBookingResponse>> GetBookingForLineAsync(
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        List<GetBookingModel> models =
            await busService.GetBookingForLineAsync(driverId, cancellationToken);

        return [.. models.Select(m => m.ToResponse())];
    }

    [HttpPut("{id}/confirm")]
    [Authorize]
    [HasBus]
    public async Task<IdResponse> ConfirmBookingAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        BookingActionResult result =
            await busService.ConfirmBookingAsync(id, driverId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(result.LineId))
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.PickedUp.ToString(), cancellationToken);

        return new IdResponse(result.BookingId);
    }

    [HttpPut("{id}/no-show")]
    [Authorize]
    [HasBus]
    public async Task<IdResponse> MarkNoShowAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        BookingActionResult result =
            await busService.MarkNoShowAsync(id, driverId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(result.LineId))
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new IdResponse(result.BookingId);
    }

    // ─── Passenger: Bookings ──────────────────────────────────────────────────

    [HttpGet("my")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<NullableResponse<MyBookingResponse>> GetMyBookingAsync(CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        MyBookingResult? result = await busService.GetMyBookingAsync(passengerId, cancellationToken);

        if (result is null)
            return new(null);

        return new(new MyBookingResponse(result.BookingId, result.LineId, result.LineName));
    }

    [HttpPost("line/{lineId}")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<IdResponse> AddBookingAsync(
        [FromRoute] int lineId,
        [FromBody] AddBookingRequest request,
        CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        int bookingId = await busService.AddBookingAsync(
            new CreateBookingModel(lineId, passengerId, request.Latitude, request.Longitude), cancellationToken);

        // Notify all active drivers on this line
        var notification = new GetBookingResponse(
            bookingId, lineId, passengerId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            request.Latitude, request.Longitude,
            BookingStatus.Waiting.ToString(),
            DateTime.UtcNow);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(lineId))
            .OnBookingAddedAsync(notification, cancellationToken);

        return new(bookingId);
    }

    [HttpDelete("cancel")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<IdResponse> CancelBookingAsync(CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        BookingActionResult result = await busService.CancelBookingAsync(passengerId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(result.LineId))
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new(result.BookingId);
    }
}
