---
status: complete
quick_id: "260620-ox6"
description: "Remove Check Status nav menu, add All Booking List to home nav with status dropdown filter and customer name/phone search for admin dashboard"
date: "2026-06-20"
---

# Quick Task 260620-ox6 — Summary

## What Changed

### Task 1: Navigation Update
- **File:** `ServiceCity/Views/Shared/_Layout.cshtml`
- Removed "Check Status" nav link (`Booking/Lookup`)
- Renamed authenticated "Bookings" link to "All Bookings" (`Admin/Dashboard`)

### Task 2: Status Filter on Admin Dashboard
- **Files:** `AdminDashboardViewModel.cs`, `AdminController.cs`, `Dashboard.cshtml`, `site.css`
- Added `StatusFilter` property to ViewModel
- Added `status` query parameter to Dashboard action with EF Core filter
- Added filter pill buttons: All / Pending / In Progress / Completed / Declined
- Preserves status filter across search via hidden field
- Shows only selected status section when filter is active
- CSS: pill-style buttons (border-radius: 20px) with 48px mobile tap targets

## Commits
1. `1422ebf` — Remove Check Status nav, rename Bookings to All Bookings
2. `6827f93` — Add status filter dropdown to admin dashboard

## Files Modified
- `ServiceCity/Views/Shared/_Layout.cshtml`
- `ServiceCity/Models/AdminDashboardViewModel.cs`
- `ServiceCity/Controllers/AdminController.cs`
- `ServiceCity/Views/Admin/Dashboard.cshtml`
- `ServiceCity/wwwroot/css/site.css`
