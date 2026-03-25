using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Buses;
using SoftPro.Wasilni.Presentation.Extensions.BusExtensions;
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
    public async Task<MutateResponse> RegisterAsync([FromBody] RegisterBusRequest registerRequest, CancellationToken cancellationToken)
    {
        int id = await busService.RegisterAsync(registerRequest.ToModel(), cancellationToken);
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
    public async Task<MutateResponse> UpdateAsync([FromRoute] IdRequest route, [FromBody] UpdateBusRequest request, CancellationToken cancellationToken)
    {
        int id = await busService.UpdateAsync(route.Id, request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<MutateResponse> DeleteAsync([FromRoute] IdRequest request, CancellationToken cancellationToken)
    {
        int id = await busService.DeleteAsync(request.Id, cancellationToken);
        return new(id);
    }

    [HttpPost("add-driver")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<GetBusesForAdminResponse> AddDriver([FromBody] AddDriverOnBusRequest request, CancellationToken cancellationToken)
    {
        GetBusesForAdminModel model = await busService.AddDriver(request.ToModel(), cancellationToken);
        return model.ToResponse();
    }

    [HttpDelete("delete-driver")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<GetBusesForAdminResponse> DeleteDriver([FromBody] DeleteDriverromBusRequest request, CancellationToken cancellationToken)
    {
        GetBusesForAdminModel model = await busService.DeleteDriver(request.ToModel(), cancellationToken);
        return model.ToResponse();
    }

}
