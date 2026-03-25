using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Domain.Models.Points;
using SoftPro.Wasilni.Presentation.Models.Request.Point;
using SoftPro.Wasilni.Presentation.Models.Response.Point;
using Twilio.Rest.Api.V2010.Account.Usage.Record;

namespace SoftPro.Wasilni.Presentation.Extensions.PointExtensions;

public static class ToModelPointExtensions
{
    public static RegisterPointModel ToModel(this RegisterPointRequest request)
    {
        return new(request.Latitude, request.Longitude);
    }

    public static GetModelPaged ToModel(this GetPointsRequest request)
    {
        return new (request.PageNumber, request.PageSize);
    }

    public static UpdatePointModel ToModel(this UpdatePointRequest request, int id)
    {
        return new(id, request.Latitude, request.Longitude);
    }
}
