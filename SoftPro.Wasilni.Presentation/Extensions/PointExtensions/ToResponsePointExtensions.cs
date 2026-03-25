using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;
using SoftPro.Wasilni.Presentation.Models.Response.Point;

namespace SoftPro.Wasilni.Presentation.Extensions.PointExtensions;

public static class ToResponsePointExtensions
{
    public static GetPointsResponse ToResponse(this GetPointsModel model)
    {
        return new(model.Id, model.Latitude, model.Longitude, model.LineId);
    }

    public static Page<GetPointsResponse> ToResponse(this Page<GetPointsModel> model)
    {
        return new(
        model.PageNumber,
        model.PageSize,
        model.TotalPages,
        model.Content.Select(x => x.ToResponse()).ToList()
        );
    }
}
