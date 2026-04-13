using Microsoft.AspNetCore.Authorization;
using SoftPro.Wasilni.Domain.Enums;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Authorization;

public class HasPermissionRequirement(Permission permission) : IAuthorizationRequirement
{
    public Permission Permission { get; } = permission;
}
