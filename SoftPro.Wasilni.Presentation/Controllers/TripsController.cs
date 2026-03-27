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
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Trip;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
[Authorize]
public class TripsController(
    ITripService tripService,
    IHubContext<TrackingHub> hubContext) : BaseController
{
    // ─── Driver ───────────────────────────────────────────────────────────────

    [HttpGet("my-active")]
    public async Task<GetTripResponse?> GetMyActiveTripAsync(CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        GetTripModel? model = await tripService.GetMyActiveTripAsync(driverId, cancellationToken);
        return model?.ToResponse();
    }

    // ─── Passenger / Admin ────────────────────────────────────────────────────

    [HttpGet("active")]
    [Authorize(Roles = $"{nameof(Role.Passenger)},{nameof(Role.Admin)}")]
    public async Task<List<GetTripResponse>> GetActiveTripsAsync(
        [FromQuery] int? lineId,
        CancellationToken cancellationToken)
    {
        List<GetTripModel> models = await tripService.GetActiveTripsAsync(lineId, cancellationToken);
        return models.Select(m => m.ToResponse()).ToList();
    }

    [HttpPost("{id}/bookings")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<GetBookingResponse> AddBookingAsync(
        [FromRoute] IdRequest route,
        [FromBody]  AddBookingRequest request,
        CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();

        GetBookingModel model = await tripService.AddBookingAsync(
            route.Id, passengerId, request.Latitude, request.Longitude, cancellationToken);

        GetBookingResponse response = model.ToResponse();

        // Notify driver in real-time
        await hubContext.Clients
            .Group(TrackingGroups.Trip(route.Id))
            .OnBookingAddedAsync(response, cancellationToken);

        return response;
    }

    [HttpDelete("{id}/bookings")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<MutateResponse> CancelBookingAsync(
        [FromRoute] IdRequest route,
        CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();

        int bookingId = await tripService.CancelBookingAsync(route.Id, passengerId, cancellationToken);

        // Notify driver in real-time
        await hubContext.Clients
            .Group(TrackingGroups.Trip(route.Id))
            .OnBookingCancelledAsync(bookingId, cancellationToken);

        return new(bookingId);
    }
}
