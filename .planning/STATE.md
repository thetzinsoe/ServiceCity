---
gsd_state_version: 1.0
milestone: v1.1
milestone_name: milestone
status: v1.1 active — Phase 8 complete
stopped_at: Phase 08 complete
last_updated: "2026-06-21T00:00:00.000Z"
progress:
  total_phases: 8
  completed_phases: 8
  total_plans: 10
  completed_plans: 10
  percent: 100
---

# ServiceCity — Project State

**Last updated:** 2026-06-20

## Project Reference

See: .planning/PROJECT.md (updated 2026-06-16)

**Core value:** Users can book AC service and know exactly when help is coming — no phone tag, no uncertainty.
**Current focus:** Phase 08 — booking-experience-polish (context gathered)

## Phase Status

| Phase | Name | Status | Plans | Progress |
|-------|------|--------|-------|----------|
| 1 | Project Scaffold + Database | ◆ Complete | 2/2 | 100% |
| 2 | Auth (Session) | ◆ Complete | 2/2 | 100% |
| 3 | User Booking | ◆ Complete | 1/1 | 100% |
| 4 | Admin Dashboard | ◆ Complete | 1/1 | 100% |
| 5 | Admin Actions + Notifications | ◆ Complete | 1/1 | 100% |
| 6 | Polish | ◆ Complete | 1/1 | 100% |
| 7 | Customer Registration | ◆ Complete | 1/1 | 100% |
| 8 | Booking Experience Polish | ◆ Complete | 2/2 | 100% |

## Recent Activity

- 2026-06-23: UI cleanup — unified page headers, broader search (name/phone/address), removed redundant Track Booking for customers, consistent style across app
- 2026-06-23: Bug fix session — 10 critical fixes (navbar sticky, booking crash, type mismatches across all admin views, address auto-fill, CSP, Settings back link)
- 2026-06-21: Phase 8 complete — Booking Experience Polish (2 plans, 5 files, autofill + dashboard overhaul)
- 2026-06-21: Phase 08 plan 01 complete — Booking Form Autofill (pre-fill name/phone/address from User entity)
- 2026-06-20: Phase 7 complete — Customer Registration & Personalized Experience (3 waves, 6 tasks, 7 commits)
  - Wave 1: Booking.UserId FK + User.Address + Migration + Customer Registration
  - Wave 2: Role-aware Home + Guest redirect + My Bookings + Lookup privacy hole closed
  - Wave 3: Admin Customers page + Role-aware nav + Inline search+pills + Badge overlap fix
- 2026-06-20: Quick task 260620-ox6 complete — Remove Check Status nav, add All Bookings with status filter + search
- 2026-06-19: Phase 02 Wave 2 complete — AdminController, SignIn/SignOut, nav conditional links
- 2026-06-19: Phase 02 code review — 4 critical, 6 warnings, 4 info findings
- 2026-06-19: Phase 02 verification — 12/16 must-haves verified, 4 need human testing
- 2026-06-19: Phase 02 UAT created — 4 manual tests pending
- 2026-06-17: Phase 01 complete — scaffold, entities, Docker, seed data

## Design Reference

See memory: `ui-design-guidelines.md` — color palette (#FFFFFF, #1877F2, #F0F2F5), mobile-first layout, card component style

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| 3-project assembly structure (Web/Core/Data) | Separation of concerns without Clean Architecture overhead | — Pending |
| Non-sequential booking reference numbers (SC-XXXXXXXX) | Prevent correlation ID leakage and business intelligence exposure | — Pending |
| Phone-only identity, no email/password | Email usage <15% in Myanmar; phone is universal | — Pending |
| In-app notifications via DB, no SignalR/WebSockets | Simpler, more resilient on spotty Myanmar mobile networks | — Pending |
| Vertical MVP slices | Each phase delivers end-to-end user capability; faster feedback | — Pending |

### Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260620-ox6 | Remove Check Status nav menu, add All Booking List to home nav with status dropdown filter and customer name/phone search for admin dashboard | 2026-06-20 | 6827f93 | [260620-ox6-remove-check-status-nav-menu-add-all-boo](./quick/260620-ox6-remove-check-status-nav-menu-add-all-boo/) |

---
*State initialized: 2026-06-16*

## Session

**Last session:** 2026-06-23T00:00:00.000Z
**Stopped at:** Confirmation page redesign + visitor nav cleanup
**Next action:** Commit changes, then decide on next milestone or wrap up

## Bug Fixes — 2026-06-23 (Session 1)

| # | Issue | Root Cause | Fix |
|---|-------|-----------|-----|
| 1 | Sticky menu didn't stick | `sticky-top` unreliable with page layout | Changed to `fixed-top` + body padding-top |
| 2 | Booking creation crash | `GenerateReferenceNumber` wrote 10 chars into `char[8]` + only 8 random bytes | Fixed array size and byte count to 10 |
| 3 | Address not auto-filled | Controller pre-filled name/phone but not address | Added `model.Address = user.Address` |
| 4 | My Bookings page crash | `@model List<Booking>` but controller returns `List<BookingListItemDto>` | Changed model type + property access |
| 5 | Track Booking validation | `[Phone]` attribute may reject Myanmar format | Removed `[Phone]`, rely on server-side validation |
| 6 | Admin Dashboard/Drilldown crash | Views used `Booking` entities but controllers return DTOs | Changed all admin views to use correct DTO types |
| 7 | Admin Details crash | `@model Booking` but controller returns `BookingDetailDto` | Changed model + `ServiceCategory.Name` → `ServiceCategoryName` |
| 8 | Admin Customers/CustomerDetail crash | Same type mismatch pattern | Changed to `CustomerListItemDto` / `BookingListItemDto` |
| 9 | Settings back link | Hardcoded to Admin Dashboard for all users | Made conditional based on user role |
| 10 | htmx CDN blocked by CSP | `script-src 'self'` blocks unpkg.com | Added `https://unpkg.com` to CSP script-src |

## UI Cleanup — 2026-06-23 (Session 2)

| # | Change | Detail |
|---|--------|--------|
| 1 | Removed Track Booking for customers | Authenticated non-admin users have "My Bookings" — Track Booking only shows for guests + admin |
| 2 | Unified page header style | All pages use `.page-header` CSS class: title + count badge + search bar in one row |
| 3 | Admin search includes address | `AdminService.QueryBookings` now searches address field; `SearchBookingsAsync` added to BookingService |
| 4 | Track Booking broader search | Admin can search by name, phone, address, reference; guests search by phone/reference |
| 5 | TrackResult shows customer name | Added Customer Name column to search results table |
| 6 | Consistent headers across app | Dashboard, Drilldown, Customers, MyBookings, Track, CustomerDetail all use same `.page-header` pattern |

## Bug Fix + Nav Cleanup — 2026-06-23 (Session 3)

| # | Issue | Fix |
|---|-------|-----|
| 1 | `varchar(12)` overflow on booking create | Reverted reference number to 8 chars (`SC-XXXXXXXX` = 11 chars, fits varchar(12)) |
| 2 | Navigation cleanup per role | **Visitor**: Home only (no service/track). **Customer**: Home + Book a Service + My Bookings. **Admin**: Home + Bookings + Customers |
| 3 | Duplicate EF Core using directive | Removed duplicate `using Microsoft.EntityFrameworkCore` in BookingService |
| 4 | EF Core version conflict in Docker | Added `Npgsql.EntityFrameworkCore.PostgreSQL` to Services csproj to align transitive deps |

## UI Update — 2026-06-23 (Session 4)

| # | Change | Detail |
|---|--------|--------|
| 1 | Removed Track Booking from visitors | Visitors only see Home + Register/Sign In — no booking features until they register |
| 2 | Confirmation page redesigned | Matches Status page style: success banner, service + info detail cards, timeline, CTA buttons |
