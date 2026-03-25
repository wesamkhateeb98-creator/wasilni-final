using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Presentation.Models.Request.Line;

namespace SoftPro.Wasilni.Presentation.Extensions.LineExtensions;

public static class ToModelExtensions
{
    public static AddLineModel ToModel(this AddLineRequest request)
        => new(request.Name);

    public static GetLineModel ToModel(this UpdateLineRequest request, int id)
        => new(id, request.Name);

    public static GetLinesFilterModel ToModel(this GetLinesRequest request)
        => new(request.PageNumber, request.PageSize, request.Name);
}
