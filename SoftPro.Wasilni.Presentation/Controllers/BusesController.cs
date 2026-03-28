using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Presentation.ActionFilters.Authorization;
using SoftPro.Wasilni.Presentation.Extensions;
using SoftPro.Wasilni.Presentation.Extensions.BusExtensions;
using SoftPro.Wasilni.Presentation.Extensions.TripExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Bus;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Bus;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
public class BusesController(IBusService busService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> AddAsync([FromBody] AddBusRequest request, CancellationToken cancellationToken)
    {
        int id = await busService.AddAsync(request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpGet]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<Page<GetBusesForAdminResponse>> GetBusesAsync([FromQuery] GetBusesForAdminRequest request, CancellationToken cancellationToken)
    {
        Page<GetBusesForAdminModel> buses = await busService.GetBusesForAdminAsync(request.ToInput(), cancellationToken);
        return buses.ToResponse();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> UpdateAsync([FromRoute] IdRequest route, [FromBody] UpdateBusRequest request, CancellationToken cancellationToken)
    {
        int id = await busService.UpdateAsync(route.Id, request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> DeleteAsync([FromRoute] IdRequest request, CancellationToken cancellationToken)
    {
        int id = await busService.DeleteAsync(request.Id, cancellationToken);
        return new(id);
    }

    [HttpPatch("{id}/driver")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> AddDriverAsync([FromRoute] IdRequest route, [FromBody] AddDriverOnBusRequest request, CancellationToken cancellationToken)
    {
        int id = await busService.AddDriverAsync(route.Id, request.DriverId, cancellationToken);
        return new(id);
    }

    [HttpDelete("{id}/driver")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> DeleteDriverAsync([FromRoute] IdRequest route, CancellationToken cancellationToken)
    {
        int id = await busService.DeleteDriverAsync(route.Id, cancellationToken);
        return new(id);
    }

    [HttpGet("my-active")]
    [Authorize]
    [HasBus]
    public async Task<GetActiveBusResponse?> GetMyActiveBusAsync(CancellationToken cancellationToken)
    {
        int driverId = User.GetId();
        GetActiveBusModel? model = await busService.GetMyActiveBusAsync(driverId, cancellationToken);
        return model?.ToResponse();
    }


    [HttpGet("active")]
    [Authorize(Roles = nameof(Role.Passenger))]
    public async Task<List<GetActiveBusResponse>> GetActiveBusesAsync(
        [FromQuery] int? lineId,
        CancellationToken cancellationToken)
    {
        List<GetActiveBusModel> models = await busService.GetActiveBusesAsync(lineId, cancellationToken);
        return models.Select(m => m.ToResponse()).ToList();
    }

}
