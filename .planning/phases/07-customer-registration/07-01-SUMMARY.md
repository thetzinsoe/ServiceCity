---
phase: 07-customer-registration
plan: 07-01
subsystem: auth, ui, database
tags: [registration, cookie-auth, ef-core, postgresql, bootstrap5, role-based-nav]

requires:
  - phase: 02-auth-session
    provides: "Cookie auth infrastructure, User entity, PasswordHasher, AuthController"
  - phase: 03-user-booking
    provides: "Booking entity, BookingController, booking creation flow"
  - phase: 04-admin-dashboard
    provides: "Admin dashboard with status filters, booking cards, search"
provides:
  - "Customer registration with phone validation and immediate sign-in"
  - "Role-aware home page (guest/customer/admin)"
  - "Booking.UserId FK linking bookings to authenticated users"
  - "My Bookings page replacing phone-based Check Status"
  - "Admin Customers page with search and booking counts"
  - "Role-aware navigation (admin/customer/guest)"
  - "Inline search bar with status pills on dashboard"
  - "Fixed booking card badge overlap"
affects: [08-*, admin-dashboard, booking-flow]

tech-stack:
  added: []
  patterns: [role-aware-nav, conditional-button-rendering, inline-filter-layout]

key-files:
  created:
    - "ServiceCity/Models/RegisterViewModel.cs"
    - "ServiceCity/Models/CustomerViewModel.cs"
    - "ServiceCity/Views/Auth/Register.cshtml"
    - "ServiceCity/Views/Booking/MyBookings.cshtml"
    - "ServiceCity/Views/Admin/Customers.cshtml"
    - "ServiceCity/Views/Admin/CustomerDetail.cshtml"
  modified:
    - "ServiceCity.Core/Entities/Booking.cs"
    - "ServiceCity.Core/Entities/User.cs"
    - "ServiceCity.Data/Migrations/"
    - "ServiceCity/Controllers/AuthController.cs"
    - "ServiceCity/Controllers/HomeController.cs"
    - "ServiceCity/Controllers/BookingController.cs"
    - "ServiceCity/Controllers/AdminController.cs"
    - "ServiceCity/Views/Shared/_Layout.cshtml"
    - "ServiceCity/Views/Home/Index.cshtml"
    - "ServiceCity/Views/Admin/Dashboard.cshtml"
    - "ServiceCity/Views/Auth/SignIn.cshtml"
    - "ServiceCity/wwwroot/css/site.css"

key-decisions:
  - "Reuse existing User entity and cookie auth for customer registration (D-02)"
  - "UserId FK is nullable so pre-registration bookings remain visible to admin (D-08)"
  - "Lookup views deleted — replaced by MyBookings for authenticated users (D-04)"
  - "Role-aware SignIn redirect: admin to Dashboard, customer to Home (D-01)"

patterns-established:
  - "Role-aware nav: @if (User.IsInRole('Admin')) / else if authenticated / else blocks in _Layout.cshtml"
  - "Conditional CTA buttons: Book Now for authenticated, Sign In to Book for guests"
  - "Inline filter layout: flex-wrap row with pills left, search right"

requirements-completed: [CUST-01]

duration: 15min
completed: 2026-06-20
status: complete
---

# Phase 07 Plan 01: Customer Registration & Personalized Experience Summary

**Customer registration with phone/password auth, role-aware home/nav, per-user booking ownership via UserId FK, admin customer directory, and dashboard UI fixes**

## Performance

- **Duration:** ~15 min (active execution)
- **Started:** 2026-06-20T19:01:01Z
- **Completed:** 2026-06-20T19:12:00Z
- **Tasks:** 6
- **Files created:** 6
- **Files modified:** 12

## Accomplishments

- Customer registration flow at /Auth/Register with name, phone, address, password — signs in immediately after creation
- Role-aware home page: guests see "Sign In to Book" buttons, customers see "Book Now", admin redirects to Dashboard
- Booking.UserId FK added — new bookings from signed-in customers are linked to their account
- My Bookings page replaces phone-based Check Status — customers see only their own bookings
- Admin Customers page with search by name/phone, booking counts, and customer detail view
- Role-aware navigation: admin gets Customers link (no Book a Service), customer gets My Bookings, guest gets Sign In
- Dashboard search bar merged inline with status pills on same row
- Booking card badge overlap fixed — badge now sits below customer name

## Task Commits

Each task was committed atomically:

1. **Task 1: Add Booking.UserId FK + User.Address + EF Core migration** - `a31c0ba` (feat)
2. **Task 2: Customer registration — RegisterViewModel, AuthController.Register, Register view** - `f253ceb` (feat)
3. **Task 3: Role-aware Home page + "Book a Service" guest redirect** - `a96fb0b` (feat)
4. **Task 4: Link bookings to UserId + My Bookings page** - `9e6ea56` (feat)
5. **Task 5: Admin Customers page — list, search, booking counts, detail view** - `e535914` (feat)
6. **Task 6: Navigation update, inline search+pills, badge overlap fix** - `c4857b5` (feat)

## Files Created/Modified

**Created:**
- `ServiceCity/Models/RegisterViewModel.cs` — Registration form model with validation (Name, PhoneNumber, Address, Password, ConfirmPassword)
- `ServiceCity/Models/CustomerViewModel.cs` — Customer list item model (Id, Name, PhoneNumber, BookingCount, LastBooking)
- `ServiceCity/Views/Auth/Register.cshtml` — Customer registration form matching SignIn card style
- `ServiceCity/Views/Booking/MyBookings.cshtml` — Customer's own bookings list with status badges and empty state
- `ServiceCity/Views/Admin/Customers.cshtml` — Admin customer card grid with search bar
- `ServiceCity/Views/Admin/CustomerDetail.cshtml` — Customer detail with info header and booking history

**Modified:**
- `ServiceCity.Core/Entities/Booking.cs` — Added nullable UserId FK + User navigation property
- `ServiceCity.Core/Entities/User.cs` — Added Address property
- `ServiceCity.Data/Migrations/` — New migration: AddBookingUserIdAndUserAddress
- `ServiceCity/Controllers/AuthController.cs` — Added Register GET/POST, role-aware SignIn redirect
- `ServiceCity/Controllers/HomeController.cs` — Added admin redirect to Dashboard
- `ServiceCity/Controllers/BookingController.cs` — Added UserId assignment, MyBookings action, guest redirect, closed Lookup hole
- `ServiceCity/Controllers/AdminController.cs` — Added Customers/CustomerDetail actions
- `ServiceCity/Views/Shared/_Layout.cshtml` — Role-aware nav links
- `ServiceCity/Views/Home/Index.cshtml` — Conditional Book Now / Sign In to Book buttons
- `ServiceCity/Views/Admin/Dashboard.cshtml` — Inline search+pills, badge overlap fix
- `ServiceCity/Views/Auth/SignIn.cshtml` — Added InfoMessage display for guest redirects
- `ServiceCity/wwwroot/css/site.css` — Added booking-card word-break rule

**Deleted:**
- `ServiceCity/Views/Booking/Lookup.cshtml` — Replaced by MyBookings
- `ServiceCity/Views/Booking/LookupResults.cshtml` — Replaced by MyBookings

## Decisions Made

- **Reuse existing User entity and cookie auth (D-02):** No new auth system needed. Registration creates a User with IsAdmin=false, same cookie auth as admin setup.
- **Nullable UserId FK (D-08):** Existing bookings (pre-registration) have null UserId. Admin still sees them in the dashboard. New bookings from signed-in users get UserId set from server-side claim.
- **Delete Lookup views (D-04):** Phone-based Check Status is a privacy hole. Replaced by MyBookings for authenticated users. Lookup actions now redirect to SignIn then MyBookings.
- **Role-aware SignIn redirect (D-01):** Admin signs in → Dashboard. Customer signs in → Home (service categories). Prevents admin from seeing customer home page.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Razor tuple named element syntax**
- **Found during:** Task 4 (MyBookings view)
- **Issue:** Razor compiler does not support named tuple element access (`.bg`, `.text`) — must use `.Item1`, `.Item2`
- **Fix:** Changed `style.bg` / `style.text` to `style.Item1` / `style.Item2` with intermediate variables
- **Files modified:** ServiceCity/Views/Booking/MyBookings.cshtml, ServiceCity/Views/Admin/CustomerDetail.cshtml
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 9e6ea56 (Task 4)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Minor syntax fix. No scope creep.

## Issues Encountered

- .NET SDK not installed on execution environment — installed via official dotnet-install.sh script
- libicu missing — resolved by setting DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
- EF Core CLI tool needed DOTNET_ROOT explicitly set

## Known Stubs

None — all views are fully wired with data sources.

## Threat Flags

| Flag | File | Description |
|------|------|-------------|
| T-07-01 | ServiceCity/Controllers/AuthController.cs | [ValidateAntiForgeryToken] on Register POST — mitigated |
| T-07-02 | ServiceCity/Controllers/BookingController.cs | UserId set from server-side ClaimTypes.NameIdentifier, never from client — mitigated |
| T-07-03 | ServiceCity/Controllers/AdminController.cs | [Authorize] on controller — mitigated |
| T-07-04 | ServiceCity/Controllers/AuthController.cs | Register always sets IsAdmin=false — mitigated |
| T-07-05 | ServiceCity/Controllers/AuthController.cs | PhoneNumberNormalized uniqueness check before user creation — mitigated |

## Self-Check

## Self-Check: PASSED

All 6 task commits verified in git log. All created files exist. Build succeeds with 0 errors.

## Next Phase Readiness

- Customer identity system complete — ready for features that depend on user context
- Admin customer directory available for customer management features
- Booking ownership established for future per-user analytics/history
- Navigation is role-aware and extensible for additional roles

---
*Phase: 07-customer-registration*
*Completed: 2026-06-20*
