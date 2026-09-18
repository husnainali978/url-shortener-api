namespace UrlShortener.Api.Dtos;

public record CreateShortUrlRequest(string Url);

public record ShortUrlResponse(string ShortCode, string ShortUrl, string LongUrl, DateTime CreatedAt);

public record ClickDto(DateTime ClickedAt, string? Referrer);

public record AnalyticsResponse(string ShortCode, string LongUrl, int TotalClicks, List<ClickDto> RecentClicks);
