using Microsoft.AspNetCore.Authorization;
using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "HasPermission.";

    public HasPermissionAttribute(Permission permission) : base($"{PolicyPrefix}{permission}") { }
}
