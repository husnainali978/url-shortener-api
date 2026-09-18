using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services;

public class ShortUrlService(AppDbContext db) : IShortUrlService
{
    // Pushes early short codes past the trivially-guessable single characters ("0", "1", "2"...)
    // that a raw, un-offset auto-increment Id would produce.
    private const long CodeOffset = 100_000;

    public async Task<ShortUrl> CreateShortUrlAsync(string longUrl, CancellationToken ct = default)
    {
        var shortUrl = new ShortUrl
        {
            LongUrl = longUrl,
            ShortCode = string.Empty, // placeholder until the row has an Id
            CreatedAt = DateTime.UtcNow
        };

        db.ShortUrls.Add(shortUrl);
        await db.SaveChangesAsync(ct); // assigns the auto-increment Id

        // The code is a deterministic function of the row's own Id, so two concurrent
        // inserts can never derive the same code - no random collisions to retry on.
        shortUrl.ShortCode = Base62Encoder.Encode(shortUrl.Id + CodeOffset);
        await db.SaveChangesAsync(ct);

        return shortUrl;
    }

    public Task<ShortUrl?> GetByShortCodeAsync(string shortCode, CancellationToken ct = default) =>
        db.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == shortCode, ct);

    public async Task RecordClickAsync(ShortUrl shortUrl, string? referrer, CancellationToken ct = default)
    {
        db.Clicks.Add(new Click
        {
            ShortUrlId = shortUrl.Id,
            ClickedAt = DateTime.UtcNow,
            Referrer = referrer
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<UrlAnalytics?> GetAnalyticsAsync(string shortCode, int recentCount = 20, CancellationToken ct = default)
    {
        var shortUrl = await db.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == shortCode, ct);
        if (shortUrl is null)
            return null;

        var totalClicks = await db.Clicks.CountAsync(c => c.ShortUrlId == shortUrl.Id, ct);

        var recentClicks = await db.Clicks
            .Where(c => c.ShortUrlId == shortUrl.Id)
            .OrderByDescending(c => c.ClickedAt)
            .Take(recentCount)
            .ToListAsync(ct);

        return new UrlAnalytics(shortUrl, totalClicks, recentClicks);
    }
}
