using UrlShortener.Api.Services;

namespace UrlShortener.Api.Endpoints;

public static class RedirectEndpoints
{
    public static IEndpointRouteBuilder MapRedirectEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{shortCode}", RedirectToLongUrl)
            .WithTags("Redirect")
            .WithName("RedirectToLongUrl")
            .WithSummary("Redirect to the original URL and record the visit as a click.")
            .ExcludeFromDescription();

        return app;
    }

    private static async Task<IResult> RedirectToLongUrl(
        string shortCode,
        IShortUrlService service,
        HttpRequest request,
        CancellationToken ct)
    {
        var shortUrl = await service.GetByShortCodeAsync(shortCode, ct);
        if (shortUrl is null)
            return Results.NotFound();

        var referrer = request.Headers.Referer.ToString();
        await service.RecordClickAsync(shortUrl, string.IsNullOrWhiteSpace(referrer) ? null : referrer, ct);

        return Results.Redirect(shortUrl.LongUrl, permanent: false);
    }
}
