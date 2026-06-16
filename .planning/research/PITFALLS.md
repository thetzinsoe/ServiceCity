# Pitfalls Research

**Domain:** AC service booking platform (single-shop, Myanmar, phone-auth, ASP.NET Core MVC)
**Researched:** 2026-06-16
**Confidence:** MEDIUM (training-data based; validated against domain patterns)

## Critical Pitfalls

### Pitfall 1: Correlation ID Leak — Using Auto-Increment IDs as Booking Reference Numbers

**What goes wrong:**
Exposing database primary keys (1, 2, 3...) as public booking reference numbers. Users see sequential IDs and can guess other bookings, access other people's booking status pages, or infer business volume (e.g., "only 15 bookings this month").

**Why it happens:**
The simplest path. EF Core gives you an `Id` column and it's natural to display it. Developers don't realize the privacy and business-intelligence implications until it's too late.

**How to avoid:**
Generate a separate, non-sequential public reference number (e.g., `SC-{year}{month}-{random 4 chars}` or `SC-{8-char base36 random}`). Use this as the lookup key, not the primary key. Keep the auto-increment `Id` internal only.

**Warning signs:**
- Route URLs contain `/Booking/Status/15` instead of `/Booking/Status/SC-202606-A7X3`
- Admin dashboard shows sequential booking numbers in a public-facing column

**Phase to address:**
Phase 1 (Schema + Scaffold) — reference number generation must be part of the Booking entity design from day one. Retrofitting is painful because all URLs and lookup flows change.

---

### Pitfall 2: Phone Number as Unvalidated Free Text

**What goes wrong:**
Accepting any string as a phone number in the booking form. Duplicate users get created (one booking with `09-123-456-789`, another with `09123456789`, another with `+959123456789`). Booking lookup by phone number fails because the formats don't match.

**Why it happens:**
"Just a text field" is faster to build. Proper phone validation and normalization feels like extra work. The problem only surfaces after real users enter data in different formats.

**How to avoid:**
Use `libphonenumber-csharp` to validate and normalize all phone inputs to E.164 format (`+959123456789`) before storage. The `User` table should have a unique index on the normalized phone number. The lookup form should accept any format and normalize before querying.

**Warning signs:**
- Multiple `User` records with variations of the same phone number
- Lookup returns "no bookings found" for a phone number the user knows they used

**Phase to address:**
Phase 2 (Auth/Session) — phone validation is part of the SessionService's SignIn flow. Must be in place before Phase 3 (User Booking) starts accepting real phone data.

---

### Pitfall 3: No Rate Limiting on Public Endpoints

**What goes wrong:**
Booking form and phone lookup endpoints have no rate limiting. A malicious actor (or a curious user) can:
- Submit thousands of fake bookings, flooding the admin dashboard
- Brute-force phone lookup to enumerate all customers in the system
- DOS the app with repeated POST requests

**Why it happens:**
Rate limiting is an "infrastructure concern" that gets deferred. Developers think "who would attack a small AC booking app?" But even accidental abuse (a user refreshing rapidly, a broken script) causes problems.

**How to avoid:**
Use ASP.NET Core built-in `AddRateLimiter()` with fixed-window policy on:
- `POST /Booking/Create` — max 5 bookings per phone per hour
- `POST /Booking/Lookup` — max 10 lookups per IP per minute
- `POST /Auth/SignIn` (admin) — max 5 attempts per IP per 10 minutes

**Warning signs:**
- Dashboard shows 50+ bookings from the same phone number in minutes
- Phone lookup endpoint getting hit rapidly from a single IP

**Phase to address:**
Phase 3 (User Booking) — rate limiting middleware should be wired up when the booking controller is created. Add it to Program.cs during the booking phase, not as a separate "security phase."

---

### Pitfall 4: Blocking Notification Creation During Status Transitions

**What goes wrong:**
NotificationService.CreateAsync() is called synchronously inline during booking status transitions. If the notification insert is slow (DB contention, full table scans), it slows down the admin's accept/decline action. In the worst case, a DB deadlock on the Notifications table prevents the admin from accepting a booking.

**Why it happens:**
It's the simplest implementation. Create the booking, create the notification, save everything in one transaction. Developers don't anticipate notification table growth because "it's just text."

**How to avoid:**
In v1, keep notifications in the same transaction (simplicity wins at single-shop scale). But add a database index on `Notification.BookingId` and `Notification.CreatedAt` from the start. If the system grows, extract notification creation to a background job (e.g., `IHostedService` with a channel/queue).

**Warning signs:**
- Admin dashboard feels slow when accepting bookings with many existing notifications
- Notification table grows to 10k+ rows without proper indexing

**Phase to address:**
Phase 5 (Admin Actions + Notifications) — notification creation is bundled here. Add the database indexes in Phase 1 (Schema) as part of the NotificationConfiguration.

---

### Pitfall 5: Admin Auth — No Session Expiry or Hardcoded in Views

**What goes wrong:**
Two related failures:
1. Admin session never expires — if the admin leaves their browser open on a shared computer, anyone can access the dashboard.
2. Admin credentials hardcoded in `appsettings.json` or directly in the controller — changing the admin password requires a code change and redeploy.

**Why it happens:**
"Single admin" mentality leads to treating auth as a checkbox, not a feature. Developers hardcode credentials for speed and forget to add session timeouts.

**How to avoid:**
- Admin credentials in environment variables or a secure config source (not appsettings.json plaintext)
- Session idle timeout: 30 minutes for admin (shorter than user sessions)
- Admin PIN/password hashed with ASP.NET Core Data Protection or bcrypt
- Add a simple "Change Password" page for the admin

**Warning signs:**
- Admin credentials visible in source control
- Admin session lasts days without re-authentication

**Phase to address:**
Phase 2 (Auth) — admin auth is part of the SessionService. The same phase that builds phone-based user auth should also build admin auth correctly.

---

### Pitfall 6: Mobile-First as an Afterthought

**What goes wrong:**
Building and testing the app on a desktop browser, then "making it responsive" at the end. Forms look cramped on mobile, tap targets are too small, modals overflow the viewport, and the admin dashboard is unusable on a phone screen.

**Why it happens:**
Developers work on desktop monitors. Chrome DevTools mobile view is an extra step. Bootstrap 5's responsive classes create a false sense of security — "it's responsive by default" — but real-world mobile usage reveals layout issues.

**How to avoid:**
- Test every page on a 360px-wide viewport during development, not just at the end
- Enforce minimum 48px tap target height on all buttons, links, and form inputs
- Admin dashboard: use card-based layout (not wide data tables) so it works on mobile
- Use Bootstrap's `table-responsive` wrapper for any tables that exceed viewport width

**Warning signs:**
- Buttons and inputs are smaller than 44px on mobile
- Admin dashboard tables require horizontal scrolling without `table-responsive`
- Status badges overlap text on narrow screens

**Phase to address:**
Phase 5 (Polish) — but mobile responsiveness should be verified throughout all phases. Add mobile QA checkpoints to Phases 3, 4, and 5 success criteria.

---

### Pitfall 7: Entity Framework Core N+1 in Booking Status Pages

**What goes wrong:**
Loading a booking with its notifications, service category, and user — each navigation property triggers a separate SQL query if not eagerly loaded. A simple status page generates 4-5 database round-trips.

**Why it happens:**
EF Core lazy loading (if enabled) or forgetting to `.Include()` navigation properties. The page renders fine in development with 3 test bookings, so the N+1 isn't noticed until production.

**How to avoid:**
- Disable lazy loading (default in EF Core unless explicitly enabled)
- Always use `.Include(b => b.Notifications).Include(b => b.ServiceCategory).Include(b => b.User)` when loading a booking for display
- Use `.AsNoTracking()` for read-only queries (status page, dashboard) for a free performance boost
- Review generated SQL with `dotnet-ef log` during development

**Warning signs:**
- Status page takes 500ms+ when other pages are fast
- SQL profiler shows repeated queries for the same pattern

**Phase to address:**
Phase 3 (User Booking) — eager loading should be standard practice from the first BookingService query. Phase 4 (Admin Dashboard) is where it becomes most visible because dashboard queries are broader.

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Single DbContext project (no Core/Data separation) | Faster initial setup, fewer files | Domain logic couples to EF Core; hard to test; refactoring painful when the system grows | Only for a prototype you plan to throw away — NOT for v1 |
| Hardcoded admin credentials in appsettings.json | Quick admin login for development | Credentials in source control; changing them requires redeploy | Never. Use env vars or User Secrets from day one. |
| Skipping ViewModel layer (pass entities to views) | 30% fewer classes initially | Over-posting vulnerabilities; view requirements leak into entities; lazy loading exceptions | Never for production. Create ViewModels for every view. |
| No explicit status transition validation | Less code in BookingService | Bookings end up in impossible states; data corruption | Never. The state machine is the core integrity rule. |
| All-in-one Program.cs with no service extraction | Fewer files, faster to code | Program.cs becomes a 500-line monolith; hard to understand startup | Never. Use extension methods per concern (AddDataLayer, AddCoreServices, etc.). |
| Skipping database indexes for "small data" | Saves 5 minutes of schema design | Queries slow down linearly as bookings accumulate | Add indexes from Phase 1 — they're cheap to declare and expensive to retrofit. |

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Npgsql + EF Core | Using the wrong Npgsql major version for EF Core 10 — version mismatch causes silent failures | Match Npgsql.EntityFrameworkCore.PostgreSQL major version to EF Core major version (10.x) |
| ASP.NET Core Session | Using in-memory session in production with multiple app instances — session data isn't shared | In-memory is fine for single-container Docker deploy. Add Redis/PostgreSQL session store if scaling to multiple containers. |
| Bootstrap 5 + jQuery | Adding jQuery just because "Bootstrap needs it" | Bootstrap 5 dropped jQuery. The existing project template includes jQuery for validation-unobtrusive — that's the only legitimate use. Don't add jQuery for new features. |
| Docker | Hardcoding connection strings in Dockerfile instead of using environment variables | Use env vars (`ASPNETCORE_CONNECTIONSTRINGS__DEFAULTCONNECTION`) in docker-compose.yml |
| EF Core Migrations | Running migrations at app startup in production (`context.Database.Migrate()`) | Run migrations as a separate deployment step or init container. Auto-migration at startup locks the DB during deployment. |

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Loading all bookings on the admin dashboard with no pagination | Dashboard page takes 2+ seconds to load | Always use `.Skip()/.Take()` pagination from day one. 20 bookings per page. | 50+ bookings |
| No index on `Booking.Status` | Dashboard "filter by status" performs a full table scan | Add index on `Booking.Status` and `Booking.CreatedAt DESC` in the initial migration | 100+ bookings |
| No index on `User.Phone` | Phone lookup does a full table scan on every booking creation (find-or-create user) | Add unique index on `User.Phone` (normalized, E.164 format) | 100+ users |
| Inline notification creation during status transitions | Accept/Decline actions feel slow | In v1: add indexes. In v1.x: move to background job if needed. | 100+ notifications per booking (unlikely at single-shop scale) |
| Rendering full booking history on every status page load | Status page loads slowly for users with many past bookings | Show only the 10 most recent notifications; add "View all" toggle or pagination | 50+ notifications per user |

## Security Mistakes

| Mistake | Risk | Prevention |
|---------|------|------------|
| No antiforgery token on POST forms | CSRF — malicious site can submit bookings on behalf of a user | ASP.NET Core MVC adds antiforgery by default for Razor views with `<form method="post">`. Don't disable it. Use `[ValidateAntiForgeryToken]` explicitly on all POST actions. |
| Booking status page accessible by ID without authentication | Anyone who guesses a booking ID can see someone else's booking details (name, phone, address) | Booking status page requires phone verification: user enters phone, sees only their bookings. Never expose `/Booking/Status/{id}` directly. |
| Phone number enumeration via lookup endpoint | An attacker can iterate phone numbers to discover all customers | Rate-limit lookup endpoint. Return the same response for "no bookings found" and "invalid phone" (don't reveal whether a phone is registered). |
| Admin dashboard accessible without HTTPS | Admin credentials transmitted in cleartext over HTTP | Enforce HTTPS redirect in production. Caddy reverse proxy handles TLS termination + auto-certificates. |
| Exception details exposed in production | Stack traces reveal internal paths, table names, and query structures | Use `app.UseExceptionHandler("/Home/Error")` in production (already in Program.cs template). Add custom error page. |

## UX Pitfalls

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| No confirmation after booking submission | User doesn't know if their booking was received. They submit again → duplicate booking. | Show a clear success page with booking reference number and "What happens next" timeline. Implement client-side duplicate submission prevention (disable button after click). |
| Admin dashboard refreshes to show updates required manually | Admin must press F5 to see new bookings. Missed bookings if they don't refresh. | Add auto-refresh meta tag or a "New bookings" indicator. v1: manual refresh is acceptable but add a prominent refresh button. |
| Long booking form without progress indication | User abandons the form halfway through. | The form is simple enough (5-6 fields) that a single page is fine. But keep it visibly short — don't add optional fields that make it look overwhelming. |
| Error messages in English for Burmese-speaking users | User doesn't understand what went wrong | Use clear, simple error messages. In v1, English is acceptable if kept simple. In v1.x, localize error messages to Burmese. |
| No "back" or "cancel" button on booking form | User feels trapped, closes the browser tab | Always provide a "Cancel" link/button that goes back to the home page. |
| Stale status page — user doesn't know if anything changed | User checks repeatedly, gets frustrated | Add a prominent "Last updated" timestamp. In v1.x, consider auto-refresh or a "Check for updates" button. |

## "Looks Done But Isn't" Checklist

- [ ] **Booking Form:** Often missing empty-state handling for service categories — verify: what happens when no categories exist in DB?
- [ ] **Phone Lookup:** Often missing normalization — verify: enter `09-123 456 789`, `09123456789`, and `+959123456789` — all should find the same user
- [ ] **Admin Dashboard:** Often missing empty state — verify: what does the admin see when there are zero bookings?
- [ ] **Status Page:** Often missing the case where a booking ID doesn't exist — verify: what happens when a user navigates to a non-existent booking?
- [ ] **Decline Flow:** Often missing the user-facing display of the decline reason — verify: what does the user see after admin declines?
- [ ] **Mobile QA:** Often tested only on desktop — verify: every page on a 360px-wide viewport before marking "done"
- [ ] **Session Expiry:** Often not tested — verify: close browser, reopen 2 days later, check if booking lookup still works for users and admin must re-authenticate
- [ ] **Duplicate Booking Prevention:** Often missing — verify: rapidly clicking "Submit Booking" should not create multiple bookings

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Sequential reference numbers leaked | MEDIUM | Add new reference number column, generate values for existing bookings, update all URLs, add redirects from old ID-based URLs. |
| Unnormalized phone numbers | HIGH | Deduplicate users (merge by normalized phone), update all foreign keys, add unique index. Data migration script required. |
| No rate limiting | LOW | Add middleware in Program.cs — takes 20 lines of code. No data migration needed. |
| Missing indexes on Booking.Status and User.Phone | LOW | Add migration with `CreateIndex` — takes 5 minutes. No code changes needed. |
| Entities passed to views (over-posting risk) | MEDIUM | Extract ViewModels from existing views, map in controller, update view @model directives. Per-view refactoring. |
| N+1 queries in booking status | LOW | Add `.Include()` calls to booking queries — local change in BookingService. Verify with SQL logging. |

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Sequential ID leak | Phase 1 (Schema) | Check booking URLs use non-sequential reference numbers |
| Unvalidated phone numbers | Phase 2 (Auth) | Run normalization test with multiple phone formats |
| No rate limiting | Phase 3 (Booking) | Verify rate limit headers in HTTP responses |
| Blocking notifications | Phase 1 (Schema) + Phase 5 (Notifications) | Check Notification table indexes; test accept speed with 1k+ notifications |
| Admin auth weaknesses | Phase 2 (Auth) | Verify credentials not in source; test session expiry |
| Mobile as afterthought | Phase 3+ (all UI phases) | Mobile QA checkpoints in each phase's success criteria |
| EF Core N+1 queries | Phase 3 (Booking) | Review generated SQL for `.Include()` coverage |

## Sources

- ASP.NET Core MVC security best practices (training knowledge)
- Entity Framework Core performance patterns (training knowledge)
- Single-tenant booking system domain patterns (training knowledge)
- Myanmar mobile-first UX considerations (training knowledge, market context from PROJECT.md)

---
*Pitfalls research for: ServiceCity AC service booking platform*
*Researched: 2026-06-16*
*Confidence: MEDIUM — domain patterns are stable; verify .NET 10-specific gotchas against current docs when web access is available*
