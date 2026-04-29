using System.Collections.Concurrent;
using System.Net;

namespace WebApiShop.MiddleWare
{
    public class RateLimitingMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleWare> _logger;
        private readonly int _limit;
        private readonly TimeSpan _window;

        private readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _clients = new();

        public RateLimitingMiddleWare(RequestDelegate next, ILogger<RateLimitingMiddleWare> logger, int limit = 100, int windowSeconds = 60)
        {
            _next = next;
            _logger = logger;
            _limit = limit;
            _window = TimeSpan.FromSeconds(windowSeconds);
        }

        public async Task Invoke(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            DateTime now = DateTime.UtcNow;

            _clients.AddOrUpdate(
                clientIp,
                _ => (1, now),
                (_, existing) =>
                {
                    if (now - existing.WindowStart >= _window)
                        return (1, now);
                    return (existing.Count + 1, existing.WindowStart);
                }
            );

            var (count, windowStart) = _clients[clientIp];

            if (count > _limit)
            {
                DateTime windowEnd = windowStart + _window;
                int retryAfter = (int)Math.Ceiling((windowEnd - now).TotalSeconds);

                _logger.LogWarning("Rate limit exceeded for IP {ClientIp}: {Count} requests in window.", clientIp, count);

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = retryAfter.ToString();
                context.Response.Headers["X-RateLimit-Limit"] = _limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = "0";
                context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(windowEnd).ToUnixTimeSeconds().ToString();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":\"Too many requests. Retry after {retryAfter} seconds.\"}}");
                return;
            }

            context.Response.OnStarting(() =>
            {
                int remaining = Math.Max(0, _limit - count);
                context.Response.Headers["X-RateLimit-Limit"] = _limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(windowStart + _window).ToUnixTimeSeconds().ToString();
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }

    public static class RateLimitingExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, int limit = 100, int windowSeconds = 60)
        {
            return builder.UseMiddleware<RateLimitingMiddleWare>(limit, windowSeconds);
        }
    }
}
