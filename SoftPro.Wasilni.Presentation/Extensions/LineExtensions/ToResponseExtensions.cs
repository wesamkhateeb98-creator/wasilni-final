using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Lines;
using SoftPro.Wasilni.Presentation.Models.Response.Line;

namespace SoftPro.Wasilni.Presentation.Extensions.LineExtensions;

public static class ToResponseExtensions
{
    public static GetLineResponse ToResponse(this GetLineModel model)
        => new(model.Id, model.Name);

    public static Page<GetLineResponse> ToResponse(this Page<GetLineModel> model)
        => new(
            model.PageNumber,
            model.PageSize,
            model.TotalPages,
            model.Content.Select(x => x.ToResponse()).ToList());
}
