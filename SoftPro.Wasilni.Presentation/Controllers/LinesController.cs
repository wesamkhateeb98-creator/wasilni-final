using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Presentation.Extensions.LineExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Line;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Line;
using SoftPro.Wasilni.Domain.Models;

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

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<IdResponse> UpdateAsync(
        [FromRoute] IdRequest route,
        [FromBody] UpdateLineRequest request,
        CancellationToken cancellationToken)
    {
        int id = await lineService.UpdateLineAsync(route.Id, request.ToModel(), cancellationToken);
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
