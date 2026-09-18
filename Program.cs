using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using UrlShortener.Api.Data;
using UrlShortener.Api.Endpoints;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=urlshortener.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<IShortUrlService, ShortUrlService>();

// Basic protection against abuse of the URL-creation endpoint: a fixed window per client
// (partitioned by remote IP), with requests over the limit rejected immediately (no queueing).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("shorten", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

// Demo-friendly startup: creates the SQLite file and schema on first run with no
// separate migration step. See README for the tradeoffs vs. EF Core migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseRateLimiter();

app.MapGet("/", () => Results.Ok(new { service = "URL Shortener API", status = "running" }))
    .ExcludeFromDescription();

app.MapShortenEndpoints();
app.MapAnalyticsEndpoints();
app.MapRedirectEndpoints(); // catch-all "/{shortCode}" - keep mapped last for readability

app.Run();
