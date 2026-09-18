using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public interface IShortUrlService
{
    Task<ShortUrl> CreateShortUrlAsync(string longUrl, CancellationToken ct = default);

    Task<ShortUrl?> GetByShortCodeAsync(string shortCode, CancellationToken ct = default);

    Task RecordClickAsync(ShortUrl shortUrl, string? referrer, CancellationToken ct = default);

    Task<UrlAnalytics?> GetAnalyticsAsync(string shortCode, int recentCount = 20, CancellationToken ct = default);
}

/// <summary>Aggregated click analytics for a single short URL.</summary>
public record UrlAnalytics(ShortUrl ShortUrl, int TotalClicks, List<Click> RecentClicks);
