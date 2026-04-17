using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.ActionFilters.Authorization;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Hubs;
using SoftPro.Wasilni.Presentation.Hubs.Helpers;
using SoftPro.Wasilni.Presentation.Models.Request.Booking;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;
using Permission = SoftPro.Wasilni.Domain.Enums.Permission;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/bookings")]
[EnableRateLimiting(RateLimitPolicies.Default)]
public class BookingsController(
    IBookingService bookingService,
    IHubContext<TrackingHub> hubContext) : BaseController
{
    // ═══════════════════════════════════════════════════════════════════════════
    // ADMIN endpoints
    // ═══════════════════════════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<Page<GetAdminBookingResponse>> GetBookingsAsync(
        [FromQuery] GetAdminBookingsRequest request,
        CancellationToken cancellationToken)
    {
        Page<GetAdminBookingModel> page = await bookingService.GetBookingsForAdminAsync(
            new(request.PageNumber, request.PageSize, request.Status, request.LineId),
            cancellationToken);

        return page.ToResponse();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DRIVER endpoints
    // ═══════════════════════════════════════════════════════════════════════════

    [HttpGet("line")]
    [Authorize]
    [HasPermission(Permission.Driver)]
    public async Task<List<GetBookingResponse>> GetBookingForLineAsync(
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        List<GetBookingModel> models =
            await bookingService.GetBookingForLineAsync(driverId, cancellationToken);

        return [.. models.Select(m => m.ToResponse())];
    }

    [HttpPut("{id}/confirm")]
    [Authorize]
    [HasPermission(Permission.Driver)]
    public async Task<IdResponse> ConfirmBookingAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        BookingActionResult result =
            await bookingService.ConfirmBookingAsync(id, driverId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(result.LineId))
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.PickedUp.ToString(), cancellationToken);

        await hubContext.Clients
            .User(result.PassengerId.ToString())
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.PickedUp.ToString(), cancellationToken);

        return new IdResponse(result.BookingId);
    }

    [HttpPut("{id}/no-show")]
    [Authorize]
    [HasPermission(Permission.Driver)]
    public async Task<IdResponse> MarkNoShowAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        BookingActionResult result =
            await bookingService.MarkNoShowAsync(id, driverId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(result.LineId))
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        await hubContext.Clients
            .User(result.PassengerId.ToString())
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new IdResponse(result.BookingId);
    }

    // ─── Passenger: Bookings ──────────────────────────────────────────────────

    [HttpGet("my")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<NullableResponse<MyBookingResponse>> GetMyBookingAsync(CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        MyBookingResult? result = await bookingService.GetMyBookingAsync(passengerId, cancellationToken);

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
        AddBookingResult result = await bookingService.AddBookingAsync(
            new CreateBookingModel(lineId, passengerId, request.Latitude, request.Longitude, request.key), cancellationToken);

        // Notify all active drivers on this line
        var notification = new GetBookingResponse(
            result.BookingId, lineId, passengerId,
            result.PassengerName,
            DateOnly.FromDateTime(DateTime.UtcNow),
            request.Latitude, request.Longitude,
            BookingStatus.Waiting.ToString(),
            DateTime.UtcNow);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(lineId))
            .OnBookingAddedAsync(notification, cancellationToken);

        return new(result.BookingId);
    }

    [HttpDelete("cancel")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<IdResponse> CancelBookingAsync(CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();
        BookingActionResult result = await bookingService.CancelBookingAsync(passengerId, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.LineBooking(result.LineId))
            .OnBookingStatusChangedAsync(result.BookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new(result.BookingId);
    }
}
