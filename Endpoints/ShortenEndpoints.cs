using UrlShortener.Api.Dtos;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Endpoints;

public static class ShortenEndpoints
{
    public static IEndpointRouteBuilder MapShortenEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/urls").WithTags("Shorten");

        group.MapPost("/", CreateShortUrl)
            .RequireRateLimiting("shorten")
            .WithName("CreateShortUrl")
            .WithSummary("Create a short URL for a given long URL.");

        group.MapGet("/{shortCode}", GetShortUrl)
            .WithName("GetShortUrl")
            .WithSummary("Look up the original URL and metadata for a short code.");

        return app;
    }

    private static async Task<IResult> CreateShortUrl(
        CreateShortUrlRequest request,
        IShortUrlService service,
        HttpRequest httpRequest,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Url) ||
            !Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["url"] = ["Url must be an absolute http:// or https:// URL."]
            });
        }

        var shortUrl = await service.CreateShortUrlAsync(uri.ToString(), ct);

        return Results.Created($"/api/urls/{shortUrl.ShortCode}", ToResponse(shortUrl, httpRequest));
    }

    private static async Task<IResult> GetShortUrl(
        string shortCode,
        IShortUrlService service,
        HttpRequest httpRequest,
        CancellationToken ct)
    {
        var shortUrl = await service.GetByShortCodeAsync(shortCode, ct);
        return shortUrl is null
            ? Results.NotFound()
            : Results.Ok(ToResponse(shortUrl, httpRequest));
    }

    private static ShortUrlResponse ToResponse(Models.ShortUrl shortUrl, HttpRequest httpRequest)
    {
        var baseUrl = $"{httpRequest.Scheme}://{httpRequest.Host}";
        return new ShortUrlResponse(
            shortUrl.ShortCode,
            $"{baseUrl}/{shortUrl.ShortCode}",
            shortUrl.LongUrl,
            shortUrl.CreatedAt);
    }
}
