using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Application.Abstracts;
using SoftPro.Wasilni.Application.Cache;
using System.Security.Claims;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Authorization;

public class HasBusAuthorizationHandler(IMemoryCache cache, IServiceScopeFactory scopeFactory, IUnitOfWork unitOfWork)
    : AuthorizationHandler<HasBusRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        HasBusRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Fail();
            return;
        }

        bool hasBus = await cache.GetOrCreateAsync(
            BusCacheKeys.HasBus(userId),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return await unitOfWork.BusRepository.HasBusAsync(userId, CancellationToken.None);
            });

        if (hasBus)
            context.Succeed(requirement);
        else
            context.Fail();
    }
}
