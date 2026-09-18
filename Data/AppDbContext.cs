using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    public DbSet<Click> Clicks => Set<Click>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasIndex(s => s.ShortCode).IsUnique();
            entity.Property(s => s.ShortCode).HasMaxLength(16).IsRequired();
            entity.Property(s => s.LongUrl).HasMaxLength(2048).IsRequired();
        });

        modelBuilder.Entity<Click>(entity =>
        {
            entity.HasOne(c => c.ShortUrl)
                  .WithMany(s => s.Clicks)
                  .HasForeignKey(c => c.ShortUrlId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(c => c.Referrer).HasMaxLength(2048);
            entity.HasIndex(c => c.ShortUrlId);
        });
    }
}
