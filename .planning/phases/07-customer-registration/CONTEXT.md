---
phase: 07
slug: customer-registration
created: 2026-06-20
---

# Phase 07 — Context & Decisions

> Decisions from `/gsd:plan-phase` discussion on 2026-06-20.

## Problem

1. **Admin Home page is useless** — admin sees service category cards (same as customers) instead of the booking dashboard
2. **Privacy hole** — anyone can look up anyone's bookings by typing their phone number in Check Status
3. **No customer identity** — bookings are unlinked; repeat customers must re-enter name/address every time
4. **Booking card badge overlap** — status badge collides with customer name in card layout
5. **Search bar wastes vertical space** — sits below status pills instead of beside them

## Decisions

| ID | Decision | Rationale |
|----|----------|-----------|
| D-01 | Role-aware Home page (3 views) | HomeController.Index checks auth + role: guest → service categories with "Sign in to book", customer → service categories with Book Now, admin → booking dashboard |
| D-02 | Customer registration via existing Auth | Reuse existing User entity (IsAdmin=false). Same AuthController with new Register action. Same cookie auth, same SignIn page. |
| D-03 | Customer sees own bookings only (My Bookings) | Replaces Check Status. Authenticated customer → "My Bookings" shows their bookings filtered by UserId. No phone input needed. |
| D-04 | Remove public Check Status (phone lookup) | Privacy improvement. Registration is required to book, so all bookings are linked to a user. No anonymous phone lookup. |
| D-05 | Admin Customers page | New /Admin/Customers — list all non-admin users with booking counts, search by name/phone, click to see customer detail with booking history |
| D-06 | Search bar inline with status pills | Same row: pills on left, search on right. Hidden status field preserves filter. Cleaner use of horizontal space. |
| D-07 | Booking card badge fix | Stack layout — status badge below customer name (not beside). Prevent overlap on long names. |
| D-08 | Booking.UserId FK | Add optional UserId foreign key to Booking. Existing bookings (pre-registration) have null UserId — admin still sees them. New bookings linked to signed-in customer. |
| D-09 | "Book a Service" redirects guests to SignIn | When not authenticated, clicking Book a Service → redirect to SignIn page with message "Sign in or create an account to book." |
| D-10 | Dev stage — no migration concerns | All phases 1-6 complete. App not in production. Free to change schema, delete/recreate data. |

## Scope

| Priority | Task | Effort |
|----------|------|--------|
| P0 | Role-aware Home page | Medium |
| P0 | Customer registration page + Register action | Medium |
| P0 | Add UserId FK to Booking + link on submit | Small |
| P0 | "My Bookings" replaces Check Status | Medium |
| P1 | Admin Customers page (list/card, search, booking count) | Medium |
| P1 | Search bar inline with status pills | Small |
| P1 | Fix booking card badge overlap | Small |
| P1 | "Book a Service" redirects guests to sign-in | Small |
| P2 | Remove "All Bookings" nav item (Home shows it) | Small |
| P2 | Update ROADMAP, STATE, REQUIREMENTS | Small |

## Out of Scope (Phase 7)

- Customer profile photo / avatar
- Password change / account recovery
- Admin calendar/schedule view (ADMIN-08 → v2)
- Quick rebooking auto-fill (BOOK-09 → v2)
- Service history by phone (BOOK-11 → v2)
