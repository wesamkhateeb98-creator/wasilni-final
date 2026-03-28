using Microsoft.AspNetCore.Authorization;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Authorization;

public class HasBusAttribute : AuthorizeAttribute
{
    public const string PolicyName = "HasBus";

    public HasBusAttribute() : base(PolicyName) { }
}
