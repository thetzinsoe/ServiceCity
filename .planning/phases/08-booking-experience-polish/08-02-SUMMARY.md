---
phase: 08-booking-experience-polish
plan: 08-02
status: complete
completed: "2026-06-21"
files_modified:
  - ServiceCity/Controllers/AdminController.cs
  - ServiceCity/Views/Admin/Dashboard.cshtml
  - ServiceCity/Views/Admin/Drilldown.cshtml
  - ServiceCity/wwwroot/css/site.css
commits: []
---

# Plan 08-02 Summary: Admin Dashboard Overhaul

## What Changed

**AdminController.cs** — Added `Drilldown(string status, string? search)` action. Loads bookings filtered to a single status with optional search. Returns the same `AdminDashboardViewModel` used by the dashboard.

**Dashboard.cshtml** — Replaced the inline status sections with 5 summary cards in a responsive grid (`row-cols-2 row-cols-md-3 row-cols-lg-5`). Each card shows status icon, label, and count. Clicking navigates to `/Admin/Drilldown/{status}`. Search bar preserved (searches all bookings).

**Drilldown.cshtml** — New view for per-status booking list. Shows back link to dashboard, status heading with count, search bar (scoped to single status via hidden field), and booking card grid reusing the existing `.booking-card` pattern. Clear link when search is active.

**site.css** — Added `.status-card` styles: centered layout, hover lift effect, icon/label/count typography.

## Verification

- `dotnet build` — 0 errors
- Dashboard shows 5 summary cards with correct counts
- Clicking a card navigates to drill-down with filtered bookings
- Drill-down search scoped to single status
- Back link returns to dashboard
- Booking cards reuse existing pattern with "New" badge

## Scope

4 files modified/created. Dashboard refactored from inline sections to card-based drill-down navigation.

---

*Completed: 2026-06-21*
