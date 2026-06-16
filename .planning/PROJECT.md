# ServiceCity

## What This Is

A web-based air conditioning service booking platform where users can schedule repairs and maintenance with a single service shop. The admin can accept, decline, or schedule visit times with in-app notifications. Built for Myanmar — phone-based, no email dependency, mobile-first responsive web.

## Core Value

Users can book AC service and know exactly when help is coming — no phone tag, no uncertainty.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] User can select a service category and book without creating an account (name + phone only)
- [ ] Admin can view all bookings in a dashboard, sorted by status
- [ ] Admin can accept a booking and set an estimated arrival time
- [ ] Admin can decline a booking with a reason
- [ ] User sees status updates on their booking page (in-app notification)
- [ ] User can check booking status using their phone number

### Out of Scope

- Multi-shop marketplace — only one admin/service shop manages all bookings in v1
- Online payments — payment is handled on-site when the technician arrives
- Mobile app — fully responsive web app first; mobile app is a future version
- User reviews or ratings — deferred to a later version

## Context

AC repair booking in Myanmar still runs on phone calls and Facebook messages — it's slow, unorganized, and frustrating for both customers and service providers. ServiceCity removes the back-and-forth by giving users a simple booking flow and giving the admin a dashboard to manage requests and schedules.

The app uses in-app notifications because email usage is low in Myanmar, and phone-based account setup keeps the barrier to entry minimal.

**Persona — Ko Min (Customer):** Runs a small electronics shop in Yangon. His office AC breaks in hot season. He needs to book a repair quickly from his phone browser without creating an account or remembering a password.

**Persona — Admin:** Runs a single AC service shop. Needs to see incoming bookings, accept/decline them, and schedule technician visits. Currently manages everything through phone calls and Facebook messages.

## Design

**Aesthetic:** Modern, clean, professional minimalist. Mobile-first, large tappable targets (min 48px).

**Colors:** White (`#FFFFFF`) background, Facebook Blue (`#1877F2`) accent for CTAs/links/navbar, light gray (`#F0F2F5`) for card groupings.

**Components:** Bootstrap 5 borderless cards with `shadow-sm`, no heavy outline boxes.

_(Full guidelines: memory/ui-design-guidelines.md)_

## Constraints

- **Tech Stack**: ASP.NET Core MVC (.NET 10) + Entity Framework Core + PostgreSQL (via Npgsql)
- **Frontend**: Razor views + Bootstrap 5 (responsive, mobile-first for Myanmar users)
- **Auth**: Phone number + name based session (no email/password in v1)
- **Notifications**: In-app notification system via a status/message model in the database
- **Hosting**: Deployed via Docker container (TBD cloud provider)
- **Architecture**: Standard MVC pattern with service layer for business logic, repository pattern for data access, EF Core migrations for schema management

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Phone-only auth, no email/password | Email usage is low in Myanmar; phone keeps barrier minimal | — Pending |
| Single-shop (not marketplace) | Scope control for v1; marketplace complexity deferred | — Pending |
| In-app notifications, no email | Email delivery unreliable for target users | — Pending |
| Razor views + Bootstrap 5 | Server-rendered, mobile-first, no SPA complexity needed | — Pending |
| No online payments in v1 | Payment infrastructure complexity; on-site collection is the norm | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-06-16 after initialization*
