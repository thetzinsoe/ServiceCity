<!-- GSD:project-start source:PROJECT.md -->

## Project

**ServiceCity**

A web-based air conditioning service booking platform where users can schedule repairs and maintenance with a single service shop. The admin can accept, decline, or schedule visit times with in-app notifications. Built for Myanmar — phone-based, no email dependency, mobile-first responsive web.

**Core Value:** Users can book AC service and know exactly when help is coming — no phone tag, no uncertainty.

### Constraints

- **Tech Stack**: ASP.NET Core MVC (.NET 10) + Entity Framework Core + PostgreSQL (via Npgsql)
- **Frontend**: Razor views + Bootstrap 5 (responsive, mobile-first for Myanmar users)
- **Auth**: Phone number + name based session (no email/password in v1)
- **Notifications**: In-app notification system via a status/message model in the database
- **Hosting**: Deployed via Docker container (TBD cloud provider)
- **Architecture**: Standard MVC pattern with service layer for business logic, repository pattern for data access, EF Core migrations for schema management

<!-- GSD:project-end -->

<!-- GSD:stack-start source:research/STACK.md -->

## Technology Stack

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

# EF Core packages (add to ServiceCity.csproj via dotnet CLI)

# Phone validation

# Logging

# EF Core CLI tool (global)

# Bootstrap 5 (via CDN in _Layout.cshtml — no npm needed for server-rendered app)

# Link: https://cdn.jsdelivr.net/npm/bootstrap@5.3/dist/css/bootstrap.min.css

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

- Add htmx for polling-based partial updates (lighter than SignalR, works on spotty connections, degrades gracefully)
- Add a `[LastChecked]` timestamp on the booking to diff-poll for changes
- Add ASP.NET Core Identity with role-based access (admin per shop)
- Add tenant isolation via `ShopId` foreign key on all entities
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

### Dockerfile Strategy

# Stage 1: Build

# Stage 2: Runtime

## Sources

- **PROJECT.md (in-project)** — HIGH confidence. Definitive source for project constraints: target framework `net10.0`, PostgreSQL via Npgsql, Bootstrap 5, phone-based auth, Docker hosting.
- **ServiceCity.csproj (in-project)** — HIGH confidence. Confirms `TargetFramework` is `net10.0` with `Microsoft.NET.Sdk.Web`.
- **Training data (Claude, cutoff ~early 2025)** — MEDIUM confidence. .NET 10 LTS release scheduled November 2025 would be stable by June 2026. EF Core and Npgsql major version alignment is a long-standing convention. Bootstrap 5.3 was the latest stable series as of early 2025. PostgreSQL 16 was the current default production version.
- **Version caveat:** All specific version numbers should be verified with `dotnet --list-sdks`, `dotnet ef --version`, and `npm view bootstrap version` when network access is available. The architectural recommendations (patterns, what to avoid) carry HIGH confidence regardless of exact patch versions.

<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->

## Conventions

Conventions not yet established. Will populate as patterns emerge during development.
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->

## Architecture

Architecture not yet mapped. Follow existing patterns found in the codebase.
<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->

## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, `.github/skills/`, or `.codex/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->

## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:

- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->

<!-- GSD:profile-start -->

## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
