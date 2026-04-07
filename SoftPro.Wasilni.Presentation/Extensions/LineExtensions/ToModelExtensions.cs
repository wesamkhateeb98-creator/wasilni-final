using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Presentation.Models.Request.Line;

namespace SoftPro.Wasilni.Presentation.Extensions.LineExtensions;

public static class ToModelExtensions
{
    public static AddLineModel ToModel(this AddLineRequest request)
        => new(request.Name, request.Points.Select(p => p.ToModel()).ToList());

    public static UpdateLineModel ToModel(this UpdateLineRequest request)
        => new(request.Name, request.Points.Select(p => p.ToModel()).ToList());

    public static Point ToModel(this WayPointRequest request)
        => new(request.Latitude, request.Longitude, request.Order);

    public static List<Point> ToModel(this UpdateLinePointsRequest request)
        => request.Points.Select(p => p.ToModel()).ToList();

    public static GetLinesFilterModel ToModel(this GetLinesRequest request)
        => new(request.PageNumber, request.PageSize, request.Name);
}
