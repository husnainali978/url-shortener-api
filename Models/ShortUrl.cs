namespace UrlShortener.Api.Models;

/// <summary>
/// A shortened URL record. The ShortCode is derived from Id via Base62 encoding
/// once the row has been persisted (and therefore has an auto-increment Id).
/// </summary>
public class ShortUrl
{
    public long Id { get; set; }

    public required string ShortCode { get; set; }

    public required string LongUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Click> Clicks { get; set; } = new List<Click>();
}
