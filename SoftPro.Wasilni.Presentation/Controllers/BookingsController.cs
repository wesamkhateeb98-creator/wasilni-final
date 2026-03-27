using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Hubs;
using SoftPro.Wasilni.Presentation.Hubs.Helpers;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route("api/v1.0/bookings")]
public class BookingsController(
    IBusService              busService,
    IHubContext<TrackingHub> hubContext) : BaseController
{
    // ═══════════════════════════════════════════════════════════════════════════
    // DRIVER endpoints
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Driver retrieves all <c>Waiting</c> bookings on their line
    /// that are within <b>40 m</b> of the bus's current GPS position.
    /// Bus location must have been sent at least once via the SignalR hub
    /// (<c>UpdateLocation</c>).
    /// </summary>
    [HttpGet("nearby")]
    [Authorize]
    public async Task<List<GetBookingResponse>> GetNearbyBookingsAsync(
        CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        List<GetBookingModel> models =
            await busService.GetNearbyBookingsAsync(driverId, cancellationToken);

        return models.Select(m => m.ToResponse()).ToList();
    }

    /// <summary>
    /// Driver confirms that a passenger boarded the bus.
    /// Marks the booking as <c>PickedUp</c>, increments daily ridership,
    /// and notifies <b>all drivers on the same line</b> to remove the booking
    /// from their nearby list.
    /// </summary>
    [HttpPut("{id}/confirm")]
    [Authorize]
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

    /// <summary>
    /// Driver marks a passenger as a no-show (didn't board).
    /// Marks the booking as <c>Cancelled</c> and notifies <b>all drivers on
    /// the same line</b> to remove it from their nearby list.
    /// </summary>
    [HttpPut("{id}/no-show")]
    [Authorize]
    public async Task<IdResponse> MarkNoShowAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var (bookingId, lineId) =
            await busService.MarkNoShowAsync(id, cancellationToken);

        await hubContext.Clients
            .Group(TrackingGroups.Line(lineId))
            .OnBookingStatusChangedAsync(bookingId, BookingStatus.Cancelled.ToString(), cancellationToken);

        return new IdResponse(bookingId);
    }
}
