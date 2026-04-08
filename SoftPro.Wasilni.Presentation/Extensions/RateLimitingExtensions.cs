using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace SoftPro.Wasilni.Presentation.Extensions;

public static class RateLimitPolicies
{
    public const string Login   = "login";
    public const string Default = "default";
}

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, ct) =>
            {
                var isArabic = context.HttpContext.Request.Headers.AcceptLanguage
                    .ToString().Contains("ar", StringComparison.OrdinalIgnoreCase);

                var message = isArabic
                    ? "لقد تجاوزت الحد المسموح به من الطلبات، حاول مجدداً بعد قليل"
                    : "Too many requests, please try again later";

                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        title  = message,
                        type   = "Too Many Requests",
                        status = 429,
                    }, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }), ct);
            };

            // ── Login / public endpoints ─ IP-based, 5 req/min ────────────────
            options.AddPolicy(RateLimitPolicies.Login, context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        Window           = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        PermitLimit      = 5,
                        QueueLimit       = 0,
                    }));

            // ── Authenticated endpoints ─ UserId-based, 30 req/min ────────────
            options.AddPolicy(RateLimitPolicies.Default, context =>
            {
                var key = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.Connection.RemoteIpAddress?.ToString()
                       ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: key,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        Window            = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        PermitLimit       = 30,
                        QueueLimit        = 0,
                    });
            });
        });
    }
}
