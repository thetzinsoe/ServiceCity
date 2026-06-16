# Project Research Summary

**Project:** ServiceCity
**Domain:** AC service booking platform (single-shop, Myanmar market, phone-first)
**Researched:** 2026-06-16
**Confidence:** MEDIUM

## Executive Summary

ServiceCity is a single-shop AC service booking web app targeting Myanmar's phone-dominant user base. The research confirms the chosen stack (ASP.NET Core MVC + EF Core + PostgreSQL + Bootstrap 5) is well-suited for this scope — server-rendered Razor views keep the frontend lightweight for mobile users on variable connections, and PostgreSQL provides strong transactional guarantees for booking state management.

The recommended architecture is a pragmatic 4-layer design (Presentation → Application → Data → Infrastructure) with three project assemblies (Web, Core, Data). Clean Architecture or CQRS would be over-engineering at this scale. The critical architectural decision is the booking **status state machine** — explicit, validated transitions (Pending→Accepted→Completed, Pending→Declined) enforced in BookingService — which is the integrity core of the entire system.

Key risks identified: sequential ID leakage (use non-sequential reference numbers from day one), unvalidated/un-normalized phone numbers causing duplicate users (enforce E.164 normalization before storage), and missing mobile-first testing throughout development (not just as a final polish step). The build order is dependency-driven: Schema → Auth → User Booking → Admin Dashboard → Admin Actions + Notifications → Polish.

## Key Findings

### Recommended Stack

ASP.NET Core MVC (.NET 10) with EF Core 10, Npgsql, PostgreSQL 16+, and Bootstrap 5.3.3. No jQuery for new features (Bootstrap 5 dropped the dependency; only keep it for validation-unobtrusive). No SignalR in v1 — polling/page refresh is simpler and more resilient on Myanmar's spotty mobile networks. No ASP.NET Core Identity — custom phone+name session auth is lighter and matches requirements.

**Core technologies:**
- ASP.NET Core MVC 10.0: Server-rendered Razor views, no SPA complexity needed
- Entity Framework Core 10.0 + Npgsql: Code-first migrations, change tracking for status transitions
- PostgreSQL 16+: JSONB for flexible metadata, strong transactional guarantees, free
- Bootstrap 5.3.3: Mobile-first responsive grid, prebuilt components (badges, modals, cards)
- libphonenumber-csharp: Phone validation/normalization to E.164 format
- Docker: Multi-stage build, app + PostgreSQL via docker-compose

### Expected Features

**Must have (table stakes — 13 features):**
- Service category selection — users pick AC service type
- Booking form (name + phone + address + description) — core data capture
- Preferred date/time selection — reduces phone tag
- Booking confirmation with reference number — users know it was received
- Booking lookup by phone number — no account needed
- Status page per booking — users see pending/accepted/declined/completed
- Admin login — secure dashboard access
- Admin dashboard: booking list by status — admin's main workspace
- Admin: accept booking + set estimated arrival time — core admin workflow
- Admin: decline booking with reason — necessary for unserviceable requests
- Admin: mark in-progress and completed — complete the lifecycle
- In-app status notification badges — users see new updates
- Mobile-responsive design — non-negotiable for Myanmar smartphone users

**Should have (competitive — 6 features, v1.x):**
- Admin cost estimate on acceptance — reduces customer anxiety
- Quick rebooking for returning phones — auto-fill from past bookings
- "Technician on the way" SMS — the one notification that justifies SMS cost
- Photo upload of AC unit — visual context for admin
- Burmese language UI (key screens) — reduce language barrier
- Admin calendar/schedule view — visual scheduling beyond list view

**Defer (v2+):**
- Multi-shop marketplace, online payments, PWA offline support, native mobile app, user reviews, chat/messaging, loyalty program

### Architecture Approach

Pragmatic 4-layer architecture with three assemblies: Web (controllers, views, ViewModels), Core (entities, service interfaces/implementations), Data (DbContext, repository, migrations, entity configs). Controllers inject service interfaces via DI; services depend on `IRepository<T>`, never on DbContext directly. ViewModels are presentation-layer only — entities never pass to views.

**Major components:**
1. **BookingService** — Booking lifecycle (create, lookup, status transitions via validated state machine)
2. **AdminService** — Dashboard queries, accept/decline/schedule actions
3. **NotificationService** — Cross-cutting status update messages, centralized notification creation
4. **SessionService** — Phone+name auth via ASP.NET Core Session, admin credential check
5. **Generic Repository\<T\>** — Thin EF Core wrapper, consistent data access pattern

### Critical Pitfalls

1. **Sequential ID leakage** — Use non-sequential public reference numbers (e.g., `SC-{random}`) from day one, not auto-increment PKs. Phase 1.
2. **Unvalidated phone numbers** — Normalize all phones to E.164 format with `libphonenumber-csharp` before storage. Unique index on normalized phone. Phase 2.
3. **No rate limiting** — Add `AddRateLimiter()` on booking creation and phone lookup endpoints. Phase 3.
4. **Mobile as afterthought** — Test every page at 360px during development, enforce 48px minimum tap targets, use card-based admin dashboard layout. All UI phases.
5. **EF Core N+1 queries** — Always use `.Include()` for navigation properties, `.AsNoTracking()` for read-only queries. Phase 3+.

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: Project Scaffold + Database
**Rationale:** Foundation — everything depends on schema and project structure
**Delivers:** Solution with 3 projects (Web/Core/Data), EF Core entities, initial migration, Docker setup
**Addresses:** All entity definitions (User, Booking, ServiceCategory, Notification)
**Avoids:** Sequential ID leak (use reference number column from start), missing indexes (add Status + Phone indexes in initial migration)

### Phase 2: Auth (Session)
**Rationale:** User identity must exist before booking creation
**Delivers:** SessionService, SignIn page (user + admin), phone validation and normalization
**Addresses:** Admin login, user phone-based identity
**Avoids:** Unvalidated phone numbers (normalize in SessionService), admin auth weaknesses (env var credentials, session timeout)

### Phase 3: User Booking
**Rationale:** Core user value — the reason the app exists
**Delivers:** BookingService, Create page, Status page, Lookup page, rate limiting middleware
**Addresses:** Booking form, confirmation, status tracking, phone lookup
**Avoids:** N+1 queries (eager loading from first query), no antiforgery (MVC default), duplicate submissions (disable button on submit)

### Phase 4: Admin Dashboard
**Rationale:** Admin needs to see bookings to manage them — depends on Phase 3 creating data
**Delivers:** AdminService, Dashboard page (bookings grouped by status), BookingDetail page
**Addresses:** Admin booking list, status filtering, pagination
**Avoids:** All-bookings-without-pagination (use Skip/Take from day one), non-responsive admin UI (card-based layout for mobile)

### Phase 5: Admin Actions + Notifications
**Rationale:** Admin workflow completes the booking lifecycle — depends on Dashboard UI and BookingService state machine
**Delivers:** Accept/Decline actions, NotificationService, status transitions, notification timeline on status page
**Addresses:** Admin booking management, user status updates, in-app notifications
**Avoids:** Inline notification issues (add Notification indexes in Phase 1), missing decline reason display (verify user sees reason)

### Phase 6: Polish
**Rationale:** UX quality after features work
**Delivers:** Responsive QA (all pages at 360px), validation UX, empty states, error pages, Burmese date/number formatting
**Addresses:** Mobile responsiveness validation, error handling, edge cases
**Avoids:** "Looks done but isn't" — verify checklist items (empty states, duplicate prevention, session expiry, mobile QA)

### Phase Ordering Rationale

- **Schema before everything** — EF Core migrations, entities, and indexes must exist before any service reads/writes
- **Auth before booking creation** — User identity (phone+name) anchors bookings. Phone normalization must be in place before bookings are created.
- **Booking creation before admin management** — Admin dashboard shows booking data. Can't build dashboard without data to display.
- **Admin actions bundled with notifications** — Accept/Decline actions create notification records. NotificationService is a dependency of both AdminService and BookingService, so they ship together.
- **Polish last** — Don't polish features that might change during development. Polish when all features are stable.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2 (Auth):** Session configuration for Docker deployment, admin credential storage pattern
- **Phase 5 (Notifications):** Database indexing strategy for notification queries at scale

Phases with standard patterns (skip research-phase):
- **Phase 1 (Schema):** Well-documented EF Core + Npgsql setup
- **Phase 3 (Booking):** Standard MVC CRUD with form validation
- **Phase 4 (Admin Dashboard):** Standard filtered/sorted list with pagination

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | MEDIUM | Stack is constraint-driven from PROJECT.md. Package versions should be verified against current NuGet when network is available. |
| Features | HIGH | Requirements well-documented in PROJECT.md and spec.md. Feature categories map clearly to domain expectations. |
| Architecture | MEDIUM | ASP.NET Core MVC patterns are stable and well-established. Specific .NET 10 idioms should be verified against current docs. |
| Pitfalls | MEDIUM | Domain patterns are stable for booking systems. .NET 10-specific gotchas unverified — check current docs when web access available. |

**Overall confidence:** MEDIUM — architectural patterns and domain knowledge are solid; version-specific details need live verification.

### Gaps to Address

- **Npgsql 10.x provider specifics:** Verify EF Core provider version, JSONB mapping, and enum mapping with Context7 or official docs during Phase 1 planning
- **Myanmar SMS gateway pricing and availability:** Research Ooredoo MEC, MPT SMS API before implementing SMS notification in v1.x
- **Burmese font rendering:** Verify Zawgyi/Unicode rendering in Bootstrap 5 on mobile Chrome for Android during Phase 3
- **.NET 10 session management changes:** Verify cookie policy defaults and session middleware behavior against .NET 10 docs during Phase 2 planning

## Sources

### Primary (HIGH confidence)
- PROJECT.md and spec.md — project constraints, requirements, personas
- ServiceCity.csproj — confirmed `net10.0` target framework
- Existing project layout (_Layout.cshtml, Program.cs, Bootstrap 5.3.3)

### Secondary (MEDIUM confidence)
- ASP.NET Core MVC, EF Core, Npgsql documentation patterns (training knowledge)
- Service booking domain patterns (training knowledge)
- Myanmar market context (training knowledge)

### Tertiary (LOW confidence)
- Specific .NET 10 package versions (need live NuGet verification)
- Myanmar SMS gateway providers (need direct research)

---
*Research completed: 2026-06-16*
*Ready for roadmap: yes*
