using Microsoft.AspNetCore.Mvc;
using SoftPro.Wasilni.Application.Abstracts.Services;
using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;
using SoftPro.Wasilni.Presentation.Extensions.PointExtensions;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;
using SoftPro.Wasilni.Presentation.Models.Request.Point;
using SoftPro.Wasilni.Presentation.Models.Response;
using SoftPro.Wasilni.Presentation.Models.Response.Point;


namespace SoftPro.Wasilni.Presentation.Controllers;

[ApiController]
[Route(BaseUrl)]
public class PointsController(IPointService pointService) : BaseController
{
    [HttpPost]
    public async Task<MutateResponse> RegisterAsync([FromBody] RegisterPointRequest request, CancellationToken cancellationToken)
    {
        int id = await pointService.RegisterAsync(request.ToModel(), cancellationToken);
        return new(id);
    }

    [HttpGet]
    public async Task<Page<GetPointsResponse>> GetPointsAsync([FromQuery] GetPointsRequest request, CancellationToken cancellationToken)
    {
        Page<GetPointsModel> points = await pointService.GetPointsForAdminAsync(request.ToModel(), cancellationToken);
        return points.ToResponse();
    }

    [HttpDelete("{id}")]
    public async Task<MutateResponse> DeleteAsync([FromRoute] IdRequest request, CancellationToken cancellationToken)
    {
        int id = await pointService.DeleteAsync(request.Id, cancellationToken);
        return new MutateResponse(id);
    }

    [HttpPut("{id}")]
    public async Task<GetPointsResponse> UpdateAsync([FromRoute] IdRequest route, [FromBody] UpdatePointRequest request, CancellationToken cancellationToken)
    {
        GetPointsModel newPoint = await pointService.UpdatePointAsync(request.ToModel(route.Id), cancellationToken);
        return newPoint.ToResponse();
    }

}
