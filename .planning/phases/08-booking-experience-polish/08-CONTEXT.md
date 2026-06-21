# Phase 8: Booking Experience Polish - Context

**Gathered:** 2026-06-20
**Status:** Ready for planning

<domain>
## Phase Boundary

Refine the booking creation experience for registered customers (autofill identity fields) and redesign the admin dashboard into a card-based status drill-down with per-status search.

Two capabilities:
1. **Booking form autofill** — Pre-fill name/phone/address from registered user's account
2. **Admin dashboard overhaul** — Replace stacked status sections with 5 summary cards that drill into per-status booking lists
</domain>

<decisions>
## Implementation Decisions

### Booking Form Autofill
- **D-01:** Identity-only pre-fill. When a registered customer visits Booking/Create, pre-fill CustomerName, CustomerPhone, and Address from their User entity (Name, PhoneNumber, Address). All fields remain editable.
- **D-02:** Description, PreferredDate, and PreferredTimeSlot are always fresh — never pre-filled. Each booking is unique in what's wrong and when.
- **D-03:** Guest (unauthenticated) flow unchanged — empty form as today. Booking/Create redirects guests to SignIn (already implemented in Phase 7).

### Admin Dashboard — Status Cards
- **D-04:** Home dashboard shows 5 summary cards at top: Pending, Accepted, In Progress, Completed, Declined. Each card shows status icon, label, and count. Clicking a card navigates to the drill-down page for that status.
- **D-05:** Each drill-down page shows all bookings for that single status as a card grid (reusing existing booking card pattern). Each card links to Admin/Details (unchanged).
- **D-06:** Drill-down pages have a back link to the dashboard. Dashboard status cards provide the summary; drill-down provides the full list.
- **D-07:** Booking detail page (Admin/Details) stays as-is — no changes needed.

### Search Behavior
- **D-08:** Home dashboard search bar searches across ALL bookings regardless of status (existing behavior, preserved).
- **D-09:** Drill-down page search bar searches only within that single status. Search param composed: `?status=Pending&search=09`.

### Navigation
- **D-10:** Admin nav unchanged — Home (redirects to Dashboard) + Customers + profile. Dashboard is the admin's landing page.
</decisions>

<canonical_refs>
## Canonical References

### Design System
- `.planning/phases/04-admin-dashboard/04-UI-SPEC.md` — Existing dashboard layout, booking card patterns, color tokens, status styles
- `.planning/phases/02-auth-session/02-UI-SPEC.md` — Auth page patterns (applicable to any new form pages)

### Existing Implementation
- `ServiceCity/Views/Admin/Dashboard.cshtml` — Current dashboard (status sections, booking cards, search, filter pills) — THIS is the primary file to refactor
- `ServiceCity/Controllers/AdminController.cs` — Dashboard action with status filter and search query params
- `ServiceCity/Models/AdminDashboardViewModel.cs` — Current view model (GroupedBookings, Counts, Search, StatusFilter)
- `ServiceCity/Views/Booking/Create.cshtml` — Booking form (add autofill here)
- `ServiceCity/Controllers/BookingController.cs` — Create GET action (add User pre-fill here)
- `ServiceCity.Core/Entities/User.cs` — Has Name, PhoneNumber, Address (source for autofill)
- `ServiceCity/wwwroot/css/site.css` — Current CSS (booking-card, status-filter styles)

### No External Specs
Requirements fully captured in decisions above.
</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **Booking cards**: `.booking-card` CSS class with hover lift effect — reuse on drill-down pages
- **Status filter pills**: `.status-filter .btn` pill styles — reuse or adapt for summary cards
- **Status styles**: Dictionary in Dashboard.cshtml with bg/text colors per status — reuse everywhere
- **Status icons**: Emoji map (🟡🔵🟠🟢🔴) in Dashboard.cshtml — reuse on summary cards
- **Card grid**: `row-cols-1 row-cols-md-2 row-cols-lg-3 g-3` responsive pattern — reuse on drill-down pages

### Established Patterns
- **Search + filter compose**: `?status=X&search=Y` URL params already supported in AdminController
- **Hidden status field**: Preserves status filter across search submissions
- **Back link**: `← Back to Bookings` pattern in Admin/Details — apply to drill-down → dashboard
- **BookingViewModel**: Already has CustomerName, CustomerPhone, Address properties — autofill uses these

### Integration Points
- **BookingController.Create GET**: Add User lookup → populate BookingViewModel identity fields
- **AdminController**: May need a new action for drill-down (`Dashboard` already supports `?status=X` — might reuse or add a dedicated cleaner route)
- **_Layout.cshtml**: No nav changes needed
</code_context>

<specifics>
## Specific Ideas

- User described wanting 4 cards initially, corrected to 5 (Pending, Accepted, In Progress, Completed, Declined) — all five BookingStatus values.
- Summary cards should be visually distinct — likely large tap targets with icon + label + count.
- Drill-down pages are essentially the current dashboard but filtered to one status automatically with no status filter pills needed (since only one status is shown).
</specifics>

<deferred>
## Deferred Ideas

- Remember last-used description/time slot for repeat bookings — user chose identity-only for now. Could revisit if customers complain about retyping the same issue.
- Combining Accepted + In Progress into one card — user explicitly wants all 5 separate.
</deferred>

---

*Phase: 08-booking-experience-polish*
*Context gathered: 2026-06-20*
