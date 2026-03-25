using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Domain.Models.Points;
using SoftPro.Wasilni.Presentation.Models.Request.Line;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;

namespace SoftPro.Wasilni.Presentation.Extensions.LineExtensions;

public static class ToModelExtensions
{
    public static AddLineModel ToModel(this AddLineRequest request)
        => new(request.Name, request.Points);

    public static GetLineModel ToModel(this UpdateLineRequest request, int id)
        => new(id, request.Name);

    public static GetLinesFilterModel ToModel(this GetLinesRequest request)
        => new(request.PageNumber, request.PageSize, request.Name);

    public static UpdateLinePointModel ToModel(this UpdateLinePointRequest request, int lineId)
        => new(lineId, request.PointId, request.Latitude, request.Longitude);

    public static AddLinePointModel ToModel(this AddLinePointRequest request, int lineId)
        => new(lineId, request.Latitude, request.Longitude);
}
