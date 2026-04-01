using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Presentation.Extensions.LineExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Line;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Line;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
[Authorize]
public class LinesController(ILineService lineService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> AddAsync(
        [FromBody] AddLineRequest request,
        CancellationToken cancellationToken)
    {
        int id = await lineService.AddLineAsync(request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpGet]
    public async Task<Page<GetLineResponse>> GetLinesAsync(
        [FromQuery] GetLinesRequest request,
        CancellationToken cancellationToken)
    {
        Page<GetLineModel> model = await lineService.GetLinesAsync(request.ToModel(), cancellationToken);
        return model.ToResponse();
    }

    [HttpGet("{id}/points")]
    public async Task<List<WayPointResponse>> GetLinePointsAsync(
        [FromRoute] IdRequest route,
        CancellationToken cancellationToken)
    {
        List<Point> points = await lineService.GetLinePointsAsync(route.Id, cancellationToken);
        return points.Select(p => p.ToResponse()).ToList();
    }

    [HttpPatch("{id}/name")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> UpdateNameAsync(
        [FromRoute] IdRequest route,
        [FromBody] UpdateLineNameRequest request,
        CancellationToken cancellationToken)
    {
        int id = await lineService.UpdateLineNameAsync(route.Id, request.Name, cancellationToken);
        return new(id);
    }

    [HttpPut("{id}/points")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> UpdatePointsAsync(
        [FromRoute] IdRequest route,
        [FromBody] UpdateLinePointsRequest request,
        CancellationToken cancellationToken)
    {
        int id = await lineService.UpdateLinePointsAsync(route.Id, request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> DeleteAsync(
        [FromRoute] IdRequest request,
        CancellationToken cancellationToken)
    {
        await lineService.DeleteLineAsync(request.Id, cancellationToken);
        return new(request.Id);
    }
}
