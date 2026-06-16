# Requirements: ServiceCity

**Defined:** 2026-06-16
**Core Value:** Users can book AC service and know exactly when help is coming — no phone tag, no uncertainty.

## v1 Requirements

Requirements for initial release. Each maps to roadmap phases.

### Service Booking

- [ ] **BOOK-01**: User can select a service category from a predefined list (repair, maintenance, installation, gas refill)
- [ ] **BOOK-02**: User can submit a booking with name, phone number, address, and problem description — no account required
- [ ] **BOOK-03**: User can select a preferred date and time slot (morning/afternoon/evening) for the service visit
- [ ] **BOOK-04**: User receives a unique, non-sequential booking reference number after submission (format: SC-XXXXXXXX)
- [ ] **BOOK-05**: User can look up their booking(s) by entering their phone number
- [ ] **BOOK-06**: User can view the status of their booking (pending, accepted, declined, completed) on a dedicated status page
- [ ] **BOOK-07**: User sees a notification timeline on their booking status page showing all status changes with timestamps
- [ ] **BOOK-08**: User sees an indicator/badge when their booking has a new status update they haven't viewed

### Admin

- [ ] **ADMIN-01**: Admin can sign in with credentials (username + password/PIN) to access the dashboard
- [ ] **ADMIN-02**: Admin can view all bookings in a dashboard, sorted by status (pending, accepted, in-progress, completed, declined)
- [ ] **ADMIN-03**: Admin can accept a pending booking and set an estimated arrival date/time
- [ ] **ADMIN-04**: Admin can decline a pending booking with a required reason
- [ ] **ADMIN-05**: Admin can mark an accepted booking as in-progress and then completed
- [ ] **ADMIN-06**: Admin can view full details of any individual booking

### Notifications

- [ ] **NOTF-01**: System automatically creates a notification when a booking is created, accepted, declined, or completed
- [ ] **NOTF-02**: User sees all notifications related to their booking on the booking status page
- [ ] **NOTF-03**: Admin's status change (accept/decline/complete) includes a message visible to the user

### Cross-Cutting

- [ ] **CROS-01**: All user-facing pages are fully usable on mobile viewports (360px minimum width)
- [ ] **CROS-02**: All buttons and form inputs have a minimum tap target of 48px height
- [ ] **CROS-03**: Burmese date and number formatting is used for user-facing dates
- [ ] **CROS-04**: Phone numbers are validated and normalized to a consistent format
- [ ] **CROS-05**: Booking submission endpoint is rate-limited to prevent abuse
- [ ] **CROS-06**: All forms include antiforgery protection

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

- **BOOK-09**: Quick rebooking — returning phone numbers auto-fill name and last address
- **BOOK-10**: Photo upload of AC unit or problem on booking form
- **BOOK-11**: Service history view by phone number
- **ADMIN-07**: Admin provides optional cost estimate when accepting a booking
- **ADMIN-08**: Admin calendar/schedule view (day/week)
- **NOTF-04**: SMS notification when technician is dispatched ("technician is on the way")
- **CROS-07**: Burmese language UI for key screens (booking form, status page)
- **CROS-08**: Dark mode for admin dashboard

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Multi-shop marketplace | Only one admin/service shop manages all bookings in v1. Marketplace adds onboarding, quality control, commission logic, search ranking, trust/safety — doubles scope. |
| Online payments (KBZ Pay, WavePay) | Requires payment gateway integration, PCI compliance, refund logic, dispute handling. On-site cash payment is the norm in Myanmar AC service. |
| Native mobile app (iOS/Android) | Fully responsive web app first. Mobile app requires separate codebase, app store distribution, and push notification infrastructure. |
| User reviews and ratings | No comparison value with a single shop. Negative reviews have no recourse. Moderation burden. Defer to multi-shop v2. |
| Email-based registration or notifications | Email usage in Myanmar is below 15%. Requiring email would kill conversion. In-app notifications on the status page replace email. |
| Real-time technician GPS tracking | Requires mobile app on technician's phone, constant GPS polling, map integration. Massive complexity for a single shop with 2-3 technicians. |
| Chat/messaging between user and admin | Adds moderation burden, response-time expectations, message storage. Status updates + admin notes cover 90% of communication. Phone calls handle the rest. |
| Social media login (Facebook, Google) | Adds OAuth dependency. Phone number is simpler and universal. Facebook API changes break login. |
| Customer membership / loyalty points | Premature optimization. Single shop with limited capacity doesn't need a loyalty program. |

## Design Guidelines

| Aspect | Specification |
|--------|---------------|
| Aesthetic | Modern, clean, professional minimalist |
| Primary background | `#FFFFFF` (white) |
| Brand accent | `#1877F2` (Facebook Blue) — used for primary CTAs, links, active states, navbar |
| Neutral tones | `#F0F2F5` (light gray) — card grouping backgrounds |
| Component style | Bootstrap 5 borderless cards with `shadow-sm`, no heavy outline boxes |
| Mobile targets | Minimum 48px height for all buttons and inputs |
| Layout priority | Mobile-first — design and test at 360px viewport width |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| BOOK-01 | Phase 3 | Pending |
| BOOK-02 | Phase 3 | Pending |
| BOOK-03 | Phase 3 | Pending |
| BOOK-04 | Phase 3 | Pending |
| BOOK-05 | Phase 3 | Pending |
| BOOK-06 | Phase 3 | Pending |
| BOOK-07 | Phase 5 | Pending |
| BOOK-08 | Phase 5 | Pending |
| ADMIN-01 | Phase 2 | Pending |
| ADMIN-02 | Phase 4 | Pending |
| ADMIN-03 | Phase 5 | Pending |
| ADMIN-04 | Phase 5 | Pending |
| ADMIN-05 | Phase 5 | Pending |
| ADMIN-06 | Phase 4 | Pending |
| NOTF-01 | Phase 5 | Pending |
| NOTF-02 | Phase 5 | Pending |
| NOTF-03 | Phase 5 | Pending |
| CROS-01 | Phase 6 | Pending |
| CROS-02 | Phase 6 | Pending |
| CROS-03 | Phase 6 | Pending |
| CROS-04 | Phase 2 | Pending |
| CROS-05 | Phase 3 | Pending |
| CROS-06 | Phase 6 | Pending |

**Coverage:**
- v1 requirements: 23 total
- Mapped to phases: 23
- Unmapped: 0 ✓

---
*Requirements defined: 2026-06-16*
*Last updated: 2026-06-16 after roadmap creation (traceability populated)*
