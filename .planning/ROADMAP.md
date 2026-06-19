# Roadmap: ServiceCity

**Created:** 2026-06-16
**Granularity:** standard
**Project Mode:** mvp (vertical slices)
**Coverage:** 23/23 v1 requirements mapped ✓

## Phase Overview

| # | Phase | Goal | Requirements | Success Criteria |
|---|-------|------|--------------|------------------|
| 1 | Project Scaffold + Database | 2/2 | Complete   | 2026-06-17 |
| 2 | Auth (Session) | 2/2 | Complete   | 2026-06-19 |
| 3 | User Booking | Booking form, reference number, phone lookup, status page | BOOK-01→06, CROS-05 | 5 |
| 4 | Admin Dashboard | Bookings grouped by status, booking detail view | ADMIN-02, ADMIN-06 | 3 |
| 5 | Admin Actions + Notifications | Accept/decline/complete lifecycle, notification timeline | ADMIN-03→05, BOOK-07→08, NOTF-01→03 | 6 |
| 6 | Polish | Mobile responsiveness, tap targets, Burmese formatting, security | CROS-01→03, CROS-06 | 4 |

---

### Phase 1: Project Scaffold + Database

**Goal:** Solution compiles, database schema exists with all entities, Docker works end-to-end.
**Mode:** mvp
**Success Criteria**:

1. `dotnet build` succeeds for all three projects (Web, Core, Data)
2. EF Core initial migration creates all entity tables (Users, Bookings, ServiceCategories, Notifications)
3. `docker compose up` starts both the app and PostgreSQL containers and the app connects to the DB
4. Database seed data populates service categories (repair, maintenance, installation, gas refill)

**Requirements:** None (foundation — entities and schema for all future phases)
**Depends on:** None
**Blocks:** Phases 2–6
**Plans:** 2/2 plans complete
Plans:
**Wave 1**

- [x] 01-01-PLAN.md — Core + Data projects, entity definitions, DbContext, migration, seed data

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 01-02-PLAN.md — Web wiring, Home page with service categories, Docker multi-project build

---

### Phase 2: Auth (Session)

**Goal:** Admin can sign in and out. Phone numbers are validated and normalized for all user identity operations.
**Mode:** mvp
**Plans:** 2/2 plans complete
**Success Criteria**:

1. Admin navigates to `/Auth/SignIn`, enters correct credentials, and is redirected to the admin dashboard (placeholder)
2. Admin can sign out from any page and is redirected to the sign-in page
3. Unauthenticated requests to `/Admin/*` redirect to the sign-in page
4. Phone number input accepts Myanmar formats (09-xxx, +959xxx) and stores normalized E.164 format
5. Invalid phone numbers (too short, letters, wrong country code) are rejected with a clear error message

**Requirements:** ADMIN-01, CROS-04
**Depends on:** Phase 1 (entities + DB)
**Blocks:** Phase 3 (booking requires user identity), Phase 4 (admin requires auth)

**Wave 1**

- [x] 02-01-PLAN.md — Admin account setup: User entity auth fields, migration, cookie auth services, Setup page with phone validation (CROS-04)

**Wave 2** *(blocked on Wave 1 completion)*

- [x] 02-02-PLAN.md — Admin sign-in/sign-out, Admin dashboard placeholder, route protection, conditional nav links (ADMIN-01)

---

### Phase 3: User Booking

**Goal:** A user can select a service category, fill out a booking form with name/phone/address/description, pick a preferred date/time, and receive a reference number. They can look up their booking by phone number and see its status.
**Mode:** mvp
**Success Criteria**:

1. User navigates to the booking form, selects a category from the dropdown, fills in all fields, selects a preferred date/time slot, and submits — receives a confirmation page with a unique SC-XXXXXXXX reference number
2. User enters their phone number on the lookup page and sees a list of all their bookings (or "No bookings found" if none exist)
3. User clicks a booking from the lookup results and sees the booking status page (status = Pending) with the booking details
4. Rapid double-clicking the submit button creates only one booking (duplicate submission prevention)
5. More than 5 booking submissions from the same phone number within 1 hour are rejected with a rate limit message

**Requirements:** BOOK-01, BOOK-02, BOOK-03, BOOK-04, BOOK-05, BOOK-06, CROS-05
**Depends on:** Phase 2 (auth + phone normalization)
**Blocks:** Phase 4 (admin needs booking data to display), Phase 5 (admin needs bookings to act on)

**UI hint:** yes — Booking creation form, confirmation page, phone lookup page, booking status page

---

### Phase 4: Admin Dashboard

**Goal:** Admin sees all bookings grouped by status on a dashboard and can click into any booking for full details.
**Mode:** mvp
**Success Criteria**:

1. Admin navigates to `/Admin/Dashboard` and sees bookings grouped into sections (Pending, Accepted, In Progress, Completed, Declined) with the newest bookings first in each section
2. Admin clicks on a booking and sees the full detail page showing all booking information (customer name, phone, address, category, description, preferred date/time, current status)
3. Dashboard is usable on a mobile viewport — sections collapse into stacked cards on narrow screens

**Requirements:** ADMIN-02, ADMIN-06
**Depends on:** Phase 2 (admin auth), Phase 3 (booking data exists to display)
**Blocks:** Phase 5 (admin actions need dashboard UI)

**UI hint:** yes — Admin dashboard with status-grouped booking lists, booking detail page

---

### Phase 5: Admin Actions + Notifications

**Goal:** Admin can accept bookings (with estimated arrival time), decline bookings (with reason), and mark them in-progress/completed. Users see a notification timeline with status badges when they check their booking.
**Mode:** mvp
**Success Criteria**:

1. Admin clicks "Accept" on a pending booking, enters an estimated arrival date/time, and the booking status changes to Accepted. The user sees the acceptance with the estimated time on their status page.
2. Admin clicks "Decline" on a pending booking, enters a required reason, and the booking status changes to Declined. The user sees the decline with the reason on their status page.
3. Admin marks an accepted booking as "In Progress" and later as "Completed." Each status change appears on the user's status page.
4. User visits their booking status page and sees a timeline of all status changes with timestamps and messages (e.g., "Booking created", "Accepted — technician arrives Wed 15 Jun, afternoon", "Completed")
5. User with unviewed status updates sees a badge/indicator on their booking list entry
6. Invalid status transitions are rejected (e.g., declining an already-accepted booking, completing a pending booking)

**Requirements:** ADMIN-03, ADMIN-04, ADMIN-05, BOOK-07, BOOK-08, NOTF-01, NOTF-02, NOTF-03
**Depends on:** Phase 3 (booking status page exists), Phase 4 (admin dashboard with booking actions)
**Blocks:** Phase 6 (polish needs all features working)

**UI hint:** yes — Accept/Decline forms on admin detail page, notification timeline on user status page, status badges

---

### Phase 6: Polish

**Goal:** All pages pass mobile QA, meet accessibility standards, use Burmese formatting, and have security hardening complete.
**Mode:** mvp
**Success Criteria**:

1. Every user-facing page renders without horizontal scroll at 360px viewport width; all content is readable and usable
2. All buttons and form inputs measure at least 48px in height on mobile viewports
3. Dates display in Burmese format (e.g., "၁၅ ဇွန် ၂၀၂၆" or DD/MM/YYYY with Burmese month names) and numbers use Myanmar formatting where appropriate
4. Every POST form includes an antiforgery token and submissions without a valid token are rejected

**Requirements:** CROS-01, CROS-02, CROS-03, CROS-06
**Depends on:** Phases 3, 4, 5 (all features must exist before polish)
**Blocks:** None (final phase)

**UI hint:** yes — responsive QA across all pages, visual polish

---

## Requirement Coverage

```
ADMIN-01 → Phase 2    ADMIN-02 → Phase 4    ADMIN-03 → Phase 5
ADMIN-04 → Phase 5    ADMIN-05 → Phase 5    ADMIN-06 → Phase 4
BOOK-01 → Phase 3     BOOK-02 → Phase 3     BOOK-03 → Phase 3
BOOK-04 → Phase 3     BOOK-05 → Phase 3     BOOK-06 → Phase 3
BOOK-07 → Phase 5     BOOK-08 → Phase 5
NOTF-01 → Phase 5     NOTF-02 → Phase 5     NOTF-03 → Phase 5
CROS-01 → Phase 6     CROS-02 → Phase 6     CROS-03 → Phase 6
CROS-04 → Phase 2     CROS-05 → Phase 3     CROS-06 → Phase 6
```

**Coverage:** 23/23 v1 requirements mapped ✓ (100%)

## Dependency Graph

```
Phase 1 (Schema + DB)
  └─→ Phase 2 (Auth)
        ├─→ Phase 3 (User Booking)
        │     ├─→ Phase 4 (Admin Dashboard)
        │     │     └─→ Phase 5 (Admin Actions + Notifications)
        │     │           └─→ Phase 6 (Polish)
        │     └─→ (booking data feeds Phase 4)
        └─→ (admin auth required for Phase 4)
```

**Parallel opportunities:** Phase 3 (User Booking) and Phase 4 (Admin Dashboard) could partially overlap — the dashboard views and query structure can be built while booking CRUD is being completed, but the dashboard needs real booking data for full verification.

---
*Roadmap created: 2026-06-16*
*Last updated: 2026-06-17 after Phase 1 planning*
