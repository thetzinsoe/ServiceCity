# ServiceCity — Project State

**Last updated:** 2026-06-16

## Project Reference

See: .planning/PROJECT.md (updated 2026-06-16)

**Core value:** Users can book AC service and know exactly when help is coming — no phone tag, no uncertainty.
**Current focus:** Phase 1 — Project Scaffold + Database

## Phase Status

| Phase | Name | Status | Plans | Progress |
|-------|------|--------|-------|----------|
| 1 | Project Scaffold + Database | ◆ In Progress | 0/3 | 0% |
| 2 | Auth (Session) | ○ Pending | 0/3 | 0% |
| 3 | User Booking | ○ Pending | 0/5 | 0% |
| 4 | Admin Dashboard | ○ Pending | 0/3 | 0% |
| 5 | Admin Actions + Notifications | ○ Pending | 0/5 | 0% |
| 6 | Polish | ○ Pending | 0/3 | 0% |

## Recent Activity

- 2026-06-16: Project initialized via `/gsd-new-project --auto`
- 2026-06-16: Research completed (stack, features, architecture, pitfalls)
- 2026-06-16: Requirements defined (23 v1 requirements across 4 categories)
- 2026-06-16: Roadmap created (6 phases, 100% requirement coverage)
- 2026-06-16: UI design guidelines saved (mobile-first, Facebook Blue accent, Bootstrap 5 shadow-sm cards)

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

---
*State initialized: 2026-06-16*
