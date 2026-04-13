using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using SoftPro.Wasilni.Domain.Exceptions;
using System.Security.Claims;

namespace SoftPro.Wasilni.Presentation.ActionFilters.Hub;

public class HubRateLimitFilter(IMemoryCache cache) : IHubFilter
{
    // Hub method name → max invocations per minute
    private static readonly Dictionary<string, int> _limits = new()
    {
        ["UpdateLocation"] = 60,
    };

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext context,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        if (_limits.TryGetValue(context.HubMethodName, out int limit))
        {
            var userId = context.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId is not null)
            {
                var window = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                var key    = $"rl:{context.HubMethodName}:{userId}:{window}";

                var count = (int)(cache.Get(key) ?? 0);

                if (count >= limit)
                {
                    var isArabic = context.Context.GetHttpContext()
                        ?.Request.Headers.AcceptLanguage
                        .ToString().Contains("ar", StringComparison.OrdinalIgnoreCase) ?? false;

                    var message = isArabic
                        ? "لقد تجاوزت الحد المسموح به من الطلبات، حاول مجدداً بعد قليل"
                        : "Too many requests, please try again later";

                    throw new TooManyRequestsException(message);
                }

                cache.Set(key, count + 1, absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(2));
            }
        }

        return await next(context);
    }
}
