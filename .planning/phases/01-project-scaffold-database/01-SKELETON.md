# Walking Skeleton — ServiceCity

**Phase:** 1
**Generated:** 2026-06-17

## Capability Proven End-to-End

A developer runs `docker compose up` and the ServiceCity web application starts, connects to PostgreSQL, applies migrations automatically, and displays a Home page with four service categories ("Repair", "Maintenance", "Installation", "Gas Refill") loaded from the database.

## Architectural Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Framework | ASP.NET Core MVC (.NET 10) | Server-rendered Razor views with full MVC pipeline. No SPA complexity needed for this domain. Tag Helpers keep templates readable. Built-in antiforgery, model validation, and response caching available when needed. |
| Data layer | Entity Framework Core 10 + Npgsql (PostgreSQL 17) | ORM with code-first migrations. LINQ queries compile to efficient SQL. Change tracker handles booking status transitions naturally. PostgreSQL is free, mature, and has excellent .NET support via Npgsql. |
| Auth | Simple session-based with phone number + name (v1) | No ASP.NET Core Identity — overkill for single-shop. No email dependency (email penetration <15% in Myanmar). Phase 2 implements sign-in; Phase 1 defines IsAdmin on User entity but enforces nothing. |
| Deployment target | Docker (Docker Compose) — cloud provider TBD | Multi-stage Docker build: SDK image for compilation, ASP.NET runtime image for execution. Docker Compose orchestrates app + PostgreSQL. Consistent dev/prod parity. |
| Directory layout | 3-project assembly under solution root | `ServiceCity.Core/` — entities, enums. `ServiceCity.Data/` — DbContext, entity configs, migrations, seeds. `ServiceCity/` (existing Web project) — controllers, views, Program.cs, wwwroot. Separation of concerns without Clean Architecture overhead. |
| Database migration strategy | Auto-migrate on startup (`db.Database.Migrate()`) | Simplest path for dev skeleton. Single-replica deployment avoids migration races. Production strategy (manual or env-gated) deferred. |
| Reference numbers | Non-sequential SC-XXXXXXXX from day 1 | Booking.ReferenceNumber column defined as unique string(12) in the schema. Random generation logic added in Phase 3. Non-sequential format prevents correlation ID leakage and business intelligence exposure. |
| Phone normalization | E.164 column added now, enforcement Phase 2 | User.PhoneNumberNormalized column (nullable string, max 20) exists in schema from initial migration. Validation and normalization logic added in Phase 2 (CROS-04). Column is nullable to support Phase 1 seed/scratch users. |
| UI framework | Bootstrap 5.3 (CDN) | Mobile-first responsive grid for Myanmar's phone-dominant user base. Carousel, modal, badge, and toast components map to booking UI needs. No npm build step — CDN link in _Layout.cshtml. |
| Logging | Default ASP.NET Core ILogger (console) | Docker captures stdout. Structured logging (Serilog) upgrade path is documented in RESEARCH.md but not wired in skeleton. |

## Stack Touched in Phase 1

- [x] Project scaffold — 3 projects (Web MVC, Core class library, Data class library), solution file, .NET 10 target framework
- [x] Routing — default MVC route: `{controller=Home}/{action=Index}/{id?}`
- [x] Database — AppDbContext with 4 entity sets, initial EF Core migration creates all tables, seed data inserts 4 categories
- [x] UI — Home page with responsive Bootstrap 5 card grid rendering service categories from PostgreSQL
- [x] Deployment — `docker compose up` starts app + PostgreSQL, auto-migrates, app serves on port 5124

## Out of Scope (Deferred to Later Slices)

- Auth/sign-in (Phase 2)
- Phone validation/normalization logic (Phase 2)
- Booking creation form and reference number generation (Phase 3)
- Booking lookup by phone number (Phase 3)
- Admin dashboard with status-grouped bookings (Phase 4)
- Admin accept/decline/complete actions (Phase 5)
- In-app notification timeline and badges (Phase 5)
- Mobile-responsive QA at 360px (Phase 6)
- Antiforgery tokens on forms (Phase 6)
- Burmese date/number formatting (Phase 6)
- Rate limiting (Phase 6)
- Serilog structured logging (future)
- Online payments (out of scope for v1)
- Multi-shop marketplace (out of scope for v1)
- Email integration (out of scope for v1)

## Subsequent Slice Plan

Each later phase adds one vertical slice on top of this skeleton without altering its architectural decisions:

- **Phase 2:** Admin can sign in and out. Phone numbers are validated and normalized to E.164 format.
- **Phase 3:** User can select a service category, fill out a booking form, submit, and receive an SC-XXXXXXXX reference number. User can look up their bookings by phone.
- **Phase 4:** Admin dashboard showing all bookings grouped by status. Admin can view full booking details.
- **Phase 5:** Admin can accept/decline/complete bookings. User sees notification timeline with status badges on their booking page.
- **Phase 6:** Mobile QA at 360px, 48px tap targets, Burmese formatting, antiforgery tokens, security hardening.
