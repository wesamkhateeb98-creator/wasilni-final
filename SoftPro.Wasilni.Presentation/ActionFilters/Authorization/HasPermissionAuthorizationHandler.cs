using Microsoft.AspNetCore.Authorization;
using SoftPro.Wasilni.Domain.Enums;
using System.Security.Claims;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Authorization;

public class HasPermissionAuthorizationHandler : AuthorizationHandler<HasPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasPermissionRequirement requirement)
    {
        // Admin always passes
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);
        if (roleClaim is not null
            && Enum.TryParse(roleClaim.Value, out Role role)
            && role == Role.Admin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var permissionClaim = context.User.FindFirst("Permission");
        if (permissionClaim is not null
            && Enum.TryParse(permissionClaim.Value, out Permission permission)
            && permission == requirement.Permission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
