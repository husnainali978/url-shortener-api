using UrlShortener.Api.Dtos;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/urls/{shortCode}/analytics", GetAnalytics)
            .WithTags("Analytics")
            .WithName("GetAnalytics")
            .WithSummary("Get total click count and recent click history for a short code.");

        return app;
    }

    private static async Task<IResult> GetAnalytics(string shortCode, IShortUrlService service, CancellationToken ct)
    {
        var analytics = await service.GetAnalyticsAsync(shortCode, recentCount: 20, ct: ct);
        if (analytics is null)
            return Results.NotFound();

        var response = new AnalyticsResponse(
            analytics.ShortUrl.ShortCode,
            analytics.ShortUrl.LongUrl,
            analytics.TotalClicks,
            analytics.RecentClicks.Select(c => new ClickDto(c.ClickedAt, c.Referrer)).ToList());

        return Results.Ok(response);
    }
}
