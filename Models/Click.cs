namespace UrlShortener.Api.Models;

/// <summary>
/// A single recorded visit to a short URL, used to build click analytics.
/// </summary>
public class Click
{
    public long Id { get; set; }

    public long ShortUrlId { get; set; }

    public ShortUrl? ShortUrl { get; set; }

    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;

    /// <summary>The HTTP Referer header, if the client sent one.</summary>
    public string? Referrer { get; set; }
}
