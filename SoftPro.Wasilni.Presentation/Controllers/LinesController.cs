using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Enums;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Domain.Models.Points;
using SoftPro.Wasilni.Presentation.Extensions.Generic;
using SoftPro.Wasilni.Presentation.Extensions.LineExtensions;
using SoftPro.Wasilni.Presentation.Extensions.PointExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Line;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Line;
using SoftPro.Wasilni.Presentation.Models.Response.Point;

namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
public class LinesController(ILineService lineService, IPointService pointService) : BaseController
{
    [HttpPost]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<MutateResponse> AddAsync([FromBody] AddLineRequest request, CancellationToken cancellationToken)
    {
        int id = await lineService.AddLine(request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<MutateResponse> DeleteLine([FromRoute] IdRequest request, CancellationToken cancellationToken)
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
    public async Task<GetLineResponse> UpdateLineAsync([FromRoute] IdRequest route, [FromBody] UpdateLineRequest request, CancellationToken cancellationToken)
    {
        GetLineModel model = await lineService.UpdateLine(request.ToModel(route.Id), cancellationToken);
        return model.ToResponse();
    }

    [HttpPost("{id}/points")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<MutateResponse> AddLinePointAsync([FromRoute] IdRequest route, [FromBody] AddLinePointRequest request, CancellationToken cancellationToken)
    {
        int pointId = await pointService.AddLinePointAsync(request.ToModel(route.Id), cancellationToken);
        return new(pointId);
    }

    [HttpGet("{id}/points")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<List<GetPointsResponse>> GetLinePointsAsync([FromRoute] IdRequest route, CancellationToken cancellationToken)
    {
        List<GetPointsModel> points = await pointService.GetLinePointsAsync(route.Id, cancellationToken);
        return points.Select(p => p.ToResponse()).ToList();
    }

    [HttpPut("{id}/points")]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<GetPointsResponse> UpdateLinePointAsync([FromRoute] IdRequest route, [FromBody] UpdateLinePointRequest request, CancellationToken cancellationToken)
    {
        GetPointsModel model = await pointService.UpdateLinePointAsync(request.ToModel(route.Id), cancellationToken);
        return model.ToResponse();
    }

}
