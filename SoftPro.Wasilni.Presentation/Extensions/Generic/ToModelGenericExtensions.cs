using SoftPro.Wasilni.Domain.Models;
using SoftPro.Wasilni.Presentation.Models.Request.Generic;

namespace SoftPro.Wasilni.Presentation.Extensions.Generic;

public static class ToModelGenericExtensions
{
    public static GetModelPaged ToModel( this GetPagedRequest request)
     => new(request.PageNumber, request.PageSize);
}
