# Stack Research

**Domain:** AC service booking web application (Myanmar, mobile-first, phone-based auth)
**Researched:** 2026-06-16
**Confidence:** MEDIUM (web search disabled in environment; versions based on training data through early 2025 and project-expected release cadences — verify with `dotnet --list-sdks` and `npm view` when connectivity is available)

## Recommended Stack

### Core Technologies

| Technology | Version | Purpose | Why Recommended |
|------------|---------|---------|-----------------|
| .NET SDK / Runtime | 10.0.x (LTS) | Application runtime and build toolchain | LTS release, 3-year support window through November 2028. Fastest .NET to date with native AOT improvements, minimal API enhancements, and Razor compiler performance gains. Project already targets `net10.0`. |
| ASP.NET Core MVC | 10.0.x | Web framework; controllers, views, routing, model binding | Server-rendered Razor views with full MVC pipeline. No SPA complexity needed for this domain. Tag Helpers keep Razor templates readable. Built-in antiforgery, model validation, and response caching. |
| Entity Framework Core | 10.0.x | ORM — database access, migrations, change tracking | Ships with .NET 10. Code-first migrations manage schema evolution cleanly. LINQ queries compile to efficient SQL. Change tracker handles booking status transitions naturally. |
| Npgsql EF Core Provider | 10.0.0 | PostgreSQL ADO.NET driver + EF Core provider | Official .NET Foundation project. Full PostgreSQL feature support (JSONB, full-text search). Connection pooling built in. Keep in sync with the EF Core major version. |
| PostgreSQL | 16.x or 17.x | Relational database | Mature, free, excellent .NET support via Npgsql. JSONB columns useful for flexible booking metadata. Strong transactional guarantees for booking state changes. Avoid 15.x and older — missing performance improvements in 16+. |
| Bootstrap | 5.3.x | CSS/JS UI framework, responsive grid, components | Mobile-first responsive grid system essential for Myanmar's phone-dominant user base. Utility classes reduce custom CSS. Dropdown, modal, badge, and toast components map directly to booking UI needs (status badges, confirmation modals, notification toasts). |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| libphonenumber-csharp | 8.13.x | Phone number validation and formatting | Validate Myanmar phone numbers on booking submission. Handles country code normalization (e.g., +959 vs 09 prefixes). Use server-side in the booking service layer. |
| Serilog.AspNetCore | 9.0.x | Structured logging | Replaces default `ILogger` with structured JSON logging. Write to console (Docker stdout) and optionally PostgreSQL. Essential for debugging booking flow issues in production. |
| Serilog.Sinks.Console | 6.0.x | Console sink for Serilog | Docker captures stdout — no file system log management needed. |
| AspNetCoreRateLimit | (evaluate) | Rate limiting middleware | Prevent booking form abuse. Apply to booking submission endpoint. **Note:** ASP.NET Core 7+ has built-in rate limiting middleware; prefer `Microsoft.AspNetCore.RateLimiting` over this third-party package unless you need IP-based client rate limiting with persistence. |
| Microsoft.AspNetCore.RateLimiting | built-in (.NET 7+) | Built-in rate limiting middleware | Use the built-in `AddRateLimiter` with fixed-window policy on booking POST endpoint. No extra NuGet package needed. |

### Development Tools

| Tool | Purpose | Notes |
|------|---------|-------|
| dotnet-ef | EF Core CLI tool for migrations | `dotnet tool install --global dotnet-ef` — generates migrations, updates database, scripts SQL. Version must match EF Core package version. |
| Docker | Containerization for deployment | Multi-stage build: SDK image for compilation, ASP.NET runtime image for execution. Docker Compose for orchestrating app + PostgreSQL containers. |
| Docker Compose | Local dev and production orchestration | Single `docker-compose.yml` defines app + PostgreSQL services. Use `.env` for connection strings, not hardcoded values. |

## Installation

```bash
# EF Core packages (add to ServiceCity.csproj via dotnet CLI)
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.*
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.*
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.*

# Phone validation
dotnet add package libphonenumber-csharp --version 8.13.*

# Logging
dotnet add package Serilog.AspNetCore --version 9.0.*

# EF Core CLI tool (global)
dotnet tool install --global dotnet-ef --version 10.0.*

# Bootstrap 5 (via CDN in _Layout.cshtml — no npm needed for server-rendered app)
# Link: https://cdn.jsdelivr.net/npm/bootstrap@5.3/dist/css/bootstrap.min.css
```

## Alternatives Considered

| Recommended | Alternative | When to Use Alternative |
|-------------|-------------|-------------------------|
| ASP.NET Core MVC | Blazor Server | If you need real-time UI updates without JavaScript. However, Blazor adds SignalR connection overhead — problematic on unreliable Myanmar mobile networks. MVC with minimal JS is more resilient. |
| PostgreSQL + Npgsql | SQL Server | If you already have SQL Server licenses or need Windows-integrated auth. No reason to pay licensing costs for this project. |
| Bootstrap 5 | Tailwind CSS | If a dedicated frontend developer wants utility-first CSS. Bootstrap provides prebuilt components (modals, badges, toasts) that map to booking UI without custom design work. |
| libphonenumber-csharp | Custom regex | If you want zero dependencies and Myanmar-only validation. libphonenumber is battle-tested by Google, handles edge cases your regex will miss (carrier prefixes, new number ranges). |
| Razor views (server-rendered) | React/Vue SPA + API | If you need offline support or highly interactive UI with no page reloads. Overkill for a booking form + status page. Razor views keep the stack simple and reduce JavaScript maintenance burden. |
| Docker | Direct IIS/Apache deploy | If you have existing Windows server infrastructure. Docker gives consistent dev/prod parity and simplifies PostgreSQL co-location. |

## What NOT to Use

| Avoid | Why | Use Instead |
|-------|-----|-------------|
| ASP.NET Core Identity (full) | Designed for email+password or external login flows. Adds user tables, password hashing, email confirmation, and 2FA complexity you explicitly don't need in v1. | Simple session-based auth with phone number as the identifier. A custom `User` model with `PhoneNumber` and `Name` stored in your own table. |
| Entity Framework Core InMemory provider | Data disappears on restart. Anyone testing locally loses their state. Makes debugging difficult. | PostgreSQL in Docker — identical to production. |
| jQuery | Bootstrap 5 dropped jQuery dependency. Adding it back pulls in unnecessary KB for Myanmar mobile users on slow connections. | Vanilla JS (Bootstrap 5 components work without jQuery) or tiny libraries like htmx if you eventually need partial updates. |
| SignalR / WebSockets (for v1) | Adds connection management and reconnection complexity. Myanmar mobile networks have frequent disconnects. | Polling or manual refresh for status updates in v1. HTTP is simpler and users expect to check manually. |
| Email-based notifications | Email penetration in Myanmar is low. Delivery to personal inboxes is unreliable. | In-app notification model stored in PostgreSQL, surfaced on the booking status page. |
| AutoMapper | Runtime mapping adds reflection overhead and obscures data flow. For a project this size (single domain, <20 entities), hand-written mapping is clearer. | Manual DTO mapping in the service layer or primary constructors on DTOs. |
| Dapper (instead of EF Core) | EF Core already handles the CRUD patterns this app needs. Dapper adds no value unless you have performance-critical read queries that EF Core can't optimize — which won't happen at single-shop booking scale. | EF Core with `.AsNoTracking()` for read-only queries when needed. |

## Stack Patterns by Variant

**If you need real-time notifications later (v2+):**
- Add htmx for polling-based partial updates (lighter than SignalR, works on spotty connections, degrades gracefully)
- Add a `[LastChecked]` timestamp on the booking to diff-poll for changes

**If you add multiple shops (marketplace v2+):**
- Add ASP.NET Core Identity with role-based access (admin per shop)
- Add tenant isolation via `ShopId` foreign key on all entities

**If you need offline capability:**
- Re-evaluate SPA (React + PWA service worker). Razor views won't suffice.

## Version Compatibility

| Package A | Compatible With | Notes |
|-----------|-----------------|-------|
| Npgsql.EntityFrameworkCore.PostgreSQL 10.0.x | EF Core 10.0.x, .NET 10 | Must match major version. Npgsql follows the same versioning as .NET/EF Core. |
| libphonenumber-csharp 8.13.x | .NET 8, .NET 9, .NET 10 | Stable across .NET versions. No runtime coupling. |
| Serilog.AspNetCore 9.0.x | .NET 9, .NET 10 | Verify compatibility via NuGet dependency graph. Minor version mismatch typically safe. |
| Bootstrap 5.3.x | Any server framework | Pure CSS/JS, no server dependency. CDN link works regardless of backend. |
| PostgreSQL 16.x | Npgsql 8.x, 9.x, 10.x | Npgsql supports multiple PostgreSQL major versions. 16 is the safe floor. |

## Infrastructure Layers

```
┌──────────────────────────────────────┐
│  Docker Host (TBD cloud provider)     │
│  ┌─────────────┐  ┌───────────────┐  │
│  │ App Container│  │ DB Container  │  │
│  │ ASP.NET 10  │  │ PostgreSQL 16 │  │
│  │ (port 8080) │  │ (port 5432)   │  │
│  └─────────────┘  └───────────────┘  │
│         │                 │           │
│         └──── internal ───┘           │
│              network                  │
└──────────────────────────────────────┘
```

### Dockerfile Strategy

```
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ServiceCity.dll"]
```

Multi-stage build keeps the runtime image small (~110 MB for ASP.NET 10 runtime vs ~800 MB SDK image).

## Sources

- **PROJECT.md (in-project)** — HIGH confidence. Definitive source for project constraints: target framework `net10.0`, PostgreSQL via Npgsql, Bootstrap 5, phone-based auth, Docker hosting.
- **ServiceCity.csproj (in-project)** — HIGH confidence. Confirms `TargetFramework` is `net10.0` with `Microsoft.NET.Sdk.Web`.
- **Training data (Claude, cutoff ~early 2025)** — MEDIUM confidence. .NET 10 LTS release scheduled November 2025 would be stable by June 2026. EF Core and Npgsql major version alignment is a long-standing convention. Bootstrap 5.3 was the latest stable series as of early 2025. PostgreSQL 16 was the current default production version.
- **Version caveat:** All specific version numbers should be verified with `dotnet --list-sdks`, `dotnet ef --version`, and `npm view bootstrap version` when network access is available. The architectural recommendations (patterns, what to avoid) carry HIGH confidence regardless of exact patch versions.

---
*Stack research for: AC service booking platform (ServiceCity)*
*Researched: 2026-06-16*
*Confidence: MEDIUM — verify package versions before running `dotnet add package`*
