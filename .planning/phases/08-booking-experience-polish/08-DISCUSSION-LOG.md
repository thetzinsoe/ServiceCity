# Phase 08 Discussion Log

**Date:** 2026-06-20
**Mode:** discuss (default)

## Area 1: Booking Form Autofill

**Q: Identity-only or last booking too?**
- Option A: Identity only — pre-fill name, phone, address from User. Description/date/time fresh.
- Option B: Last booking too — also pre-fill description and time slot from recent booking.
- **Selected:** Identity only (A). Rationale: "what's wrong" and "when" are unique per booking.

## Area 2: Admin Dashboard Status Cards

**Q: 4 or 5 status cards?**
- User initially described 4 cards, but self-corrected: wants all 5 (Pending, Accepted, In Progress, Completed, Declined).
- **Selected:** 5 cards.

**Q: Booking detail page changes?**
- **Selected:** No — Admin/Details stays as-is.

**Q: Search behavior — home dashboard vs drill-down?**
- Option A: Home searches all, drill-down searches single status.
- Option B: Both search all statuses.
- **Selected:** Home = all, drill = one (A). Clean separation.

## Summary

| # | Decision | Choice |
|---|----------|--------|
| D-01 | Autofill scope | Identity only (name, phone, address) |
| D-02 | Fresh fields | Description, date, time always empty |
| D-03 | Guest flow | Unchanged (already redirects to SignIn) |
| D-04 | Dashboard layout | 5 summary cards: Pending, Accepted, In Progress, Completed, Declined |
| D-05 | Drill-down content | Booking card grid for single status |
| D-06 | Navigation | Back link from drill-down to dashboard |
| D-07 | Detail page | No changes |
| D-08 | Home search | All statuses |
| D-09 | Drill-down search | Single status only |
| D-10 | Admin nav | Unchanged |
