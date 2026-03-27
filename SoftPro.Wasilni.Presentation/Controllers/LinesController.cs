using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Domain.Models.Trips;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.Generic;
using SoftPro.Wasilni.Presentation.Extensions.LineExtensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Hubs;
using SoftPro.Wasilni.Presentation.Hubs.Helpers;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Line;
using SoftPro.Wasilni.Presentation.Models.Request.Trip;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Line;
using SoftPro.Wasilni.Presentation.Models.Response.Trip;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
public class LinesController(
    ILineService lineService,
    IBusService busService,
    IHubContext<TrackingHub> hubContext) : BaseController
{
    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> AddAsync([FromBody] AddLineRequest request, CancellationToken cancellationToken)
    {
        int id = await lineService.AddLine(request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> DeleteLine([FromRoute] IdRequest request, CancellationToken cancellationToken)
    {
        await lineService.DeleteLine(request.Id, cancellationToken);
        return new(request.Id);
    }

    [HttpGet]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<Page<GetLineResponse>> GetLinesAsync([FromQuery] GetLinesRequest request, CancellationToken cancellationToken)
    {
        Page<GetLineModel> model = await lineService.GetLinesAsync(request.ToModel(), cancellationToken);
        return model.ToResponse();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> UpdateLineAsync([FromRoute] IdRequest route, [FromBody] UpdateLineRequest request, CancellationToken cancellationToken)
    {
        int id = await lineService.UpdateLine(request.ToModel(route.Id), cancellationToken);
        return new(id);
    }

    // ─── Passenger ────────────────────────────────────────────────────────────

    [HttpPost("{id}/bookings")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<GetBookingResponse> AddBookingAsync(
        [FromRoute] IdRequest route,
        [FromBody]  AddBookingRequest request,
        CancellationToken cancellationToken)
    {
        int passengerId = User.GetId();

        GetBookingModel model = await busService.AddBookingAsync(
            route.Id, passengerId, request.Latitude, request.Longitude, cancellationToken);

        GetBookingResponse response = model.ToResponse();

        await hubContext.Clients
            .Group(TrackingGroups.Line(route.Id))
            .OnBookingAddedAsync(response, cancellationToken);

        return response;
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
            .OnBookingCancelledAsync(bookingId, cancellationToken);

        return new(bookingId);
    }
}
