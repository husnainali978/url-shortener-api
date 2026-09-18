# URL Shortener API

A URL shortener built with **ASP.NET Core Minimal APIs**. Exists to show the skills that
show up in most real backend work: designing a small REST surface well, keeping endpoint
code out of `Program.cs`, and making deliberate calls on persistence, ID generation, and
abuse protection instead of reaching for the first library that does it.

## Architecture

No controllers - routes are grouped into extension methods over `IEndpointRouteBuilder`
and mapped from `Program.cs`, which stays a thin composition root.

```
Program.cs                 → DI wiring, rate limiter config, endpoint mapping
Endpoints/
├── ShortenEndpoints.cs     → POST /api/urls, GET /api/urls/{shortCode}
├── RedirectEndpoints.cs    → GET /{shortCode} (redirect + click logging)
└── AnalyticsEndpoints.cs   → GET /api/urls/{shortCode}/analytics
Services/
├── Base62Encoder.cs        → integer <-> Base62 string, no external deps
└── ShortUrlService.cs      → short code generation, click recording, analytics
Data/AppDbContext.cs        → EF Core DbContext (SQLite)
Models/                     → ShortUrl, Click entities
Dtos/                       → request/response records
```

**Short code generation**: a new `ShortUrl` row is inserted first so SQLite assigns it an
auto-increment `Id`, then that `Id` (plus a fixed offset, so early codes aren't literally
`"1"`, `"2"`, ...) is Base62-encoded into the `ShortCode` and saved back. Because the code
is a deterministic function of a unique row Id, there's nothing to collide on and nothing
to retry - no random generation, no GUID substrings.

**Rate limiting**: `POST /api/urls` is capped with ASP.NET Core's built-in
`Microsoft.AspNetCore.RateLimiting` middleware (fixed window, partitioned per client),
so the endpoint that writes to the database is the one that's protected from abuse.

## Features

- Create a short code for any absolute `http(s)` URL
- Visiting a short URL issues a 302 redirect and logs a click (timestamp + referrer, if sent)
- Analytics endpoint: total click count and the most recent click history for a code
- Custom Base62 short code generator (own implementation, not a GUID slice)
- EF Core + SQLite - single-file database, nothing to install or run separately
- Fixed-window rate limiting on URL creation via `Microsoft.AspNetCore.RateLimiting`
- Basic input validation with RFC-compliant absolute URL checks

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/urls` | Create a short URL (rate limited) |
| GET | `/api/urls/{shortCode}` | Get metadata for a short code |
| GET | `/{shortCode}` | Redirect to the original URL, record a click |
| GET | `/api/urls/{shortCode}/analytics` | Total clicks + recent click history |

## How to run it

```bash
dotnet restore
dotnet run
```

The SQLite database (`urlshortener.db`) is created automatically on first run via
`EnsureCreated()` - no separate migration step needed for a project this size (swap in
EF Core Migrations if you're extending the schema over time). The app listens on
`http://localhost:5080` by default (see `Properties/launchSettings.json`).

Try it with the requests in [UrlShortener.Api.http](UrlShortener.Api.http).

## Tech stack

C# · ASP.NET Core Minimal APIs · Entity Framework Core · SQLite · Microsoft.AspNetCore.RateLimiting
