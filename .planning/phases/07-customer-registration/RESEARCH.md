---
phase: 07
slug: customer-registration
created: 2026-06-20
---

# Phase 07 — Research

## 1. Current Auth Architecture

**User entity** (`ServiceCity.Core/Entities/User.cs`):
```
Id, Name, PhoneNumber, PhoneNumberNormalized, Username, PasswordHash, IsAdmin, CreatedAt
```

Already has `IsAdmin` flag — no schema change needed for users. Same entity serves both admin and customers.

**Auth flow** (cookie auth via `Microsoft.AspNetCore.Authentication.Cookies`):
- `Auth/SignIn` → POST → verifies password hash → creates `ClaimsPrincipal` with `NameIdentifier`, `Name`, and optional `Role=Admin`
- `Auth/SignOut` → clears cookie → redirects to SignIn
- `Auth/Setup` → one-time admin creation (stores `IsAdmin=true`, sets password)
- No existing customer registration action

**Authorization**:
- `[Authorize]` on AdminController (no role check, just authentication)
- `HomeController`, `BookingController` — no `[Authorize]` attribute (public)

## 2. Current Booking Flow

**Booking entity** (`ServiceCity.Core/Entities/Booking.cs`):
No `UserId` foreign key — bookings are anonymous, linked only by phone number string.

**BookingController.Create** (POST):
- Rate-limited (`BookingSubmission` policy, 5/hour by IP)
- Stores `CustomerName`, `CustomerPhone`, `CustomerPhoneNormalized`, `Address`
- Generates `IdempotencyKey` (GUID) to prevent duplicate submissions
- No user association

**BookingController.Lookup** (GET/POST):
- Public — no auth required
- GET: shows phone input form
- POST: validates phone, queries `Bookings.Where(b => b.CustomerPhoneNormalized == normalizedPhone)`
- Returns ALL bookings for that phone number ← PRIVACY ISSUE

## 3. Required Schema Changes

### Booking.cs — Add UserId FK
```
+ public int? UserId { get; set; }
+ public User? User { get; set; }
```
- Nullable: existing bookings (pre-registration) have `UserId = null`
- New bookings from signed-in customers get `UserId` set
- No data loss — admin still sees all bookings regardless of UserId

### Migration
- EF Core migration: `dotnet ef migrations add AddBookingUserId`
- Single column addition — no complex data migration needed

## 4. Registration Flow Design

### Registration page (`/Auth/Register`)
- Fields: Name (required), Phone (required, validated), Address (required), Password (min 6 chars), Confirm Password
- Creates User with `IsAdmin=false`
- Signs user in immediately after registration
- Redirects to Home (which now shows service categories for customers)

### SignIn — role-aware redirect
```
Current:        Always redirects to Admin/Dashboard
Proposed:       if (user.IsAdmin) → Admin/Dashboard
                else → Home/Index (service categories)
```

### Nav conditional rendering
```
Signed out:     [Home] [Book a Service] ... [Sign In]
Customer:       [Home] [Book a Service] [My Bookings] ... [👤 Name ▾]
Admin:          [Home] [Bookings] [Customers] ... [👤 Name ▾]
```

## 5. Home Page — 3-Way Role Switch

**HomeController.Index** logic:
```csharp
if (!User.Identity.IsAuthenticated)
    → Return View("Index", categories);  // guest view: "Sign in to book"
else if (User.IsInRole("Admin"))
    → Return RedirectToAction("Dashboard", "Admin");  // admin: booking list
else
    → Return View("Index", categories);  // customer: "Book Now" buttons work
```

View changes for guest vs customer:
- **Guest:** "Book Now" buttons replaced with "Sign In to Book" → links to Auth/SignIn
- **Customer:** "Book Now" buttons link to Booking/Create (as today)

## 6. "My Bookings" Page

**New controller action** — could be BookingController.MyBookings or new action:
```csharp
[Authorize]
public async Task<IActionResult> MyBookings()
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    var bookings = await db.Bookings
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.CreatedAt)
        .ToListAsync();
    return View(bookings);
}
```
- No phone lookup needed
- Simple list view of customer's own bookings
- Each booking links to existing Booking/Status page

## 7. Admin Customers Page

**New controller action** — AdminController.Customers:
```csharp
[Authorize]
public async Task<IActionResult> Customers(string? search)
{
    var query = db.Users.Where(u => !u.IsAdmin).AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(u => 
            u.Name.Contains(search) || 
            u.PhoneNumber.Contains(search) || 
            (u.PhoneNumberNormalized != null && u.PhoneNumberNormalized.Contains(search)));
    }
    var customers = await query
        .Select(u => new CustomerViewModel {
            Id = u.Id,
            Name = u.Name,
            PhoneNumber = u.PhoneNumber,
            BookingCount = db.Bookings.Count(b => b.UserId == u.Id),
            LastBooking = db.Bookings.Where(b => b.UserId == u.Id)
                .OrderByDescending(b => b.CreatedAt).Select(b => b.CreatedAt).FirstOrDefault()
        })
        .OrderByDescending(c => c.BookingCount)
        .ToListAsync();
    return View(customers);
}
```

**View:** Card grid (like booking cards) showing customer name, phone, booking count, last booking date. Click → customer detail with all their bookings.

## 8. Search Bar Inline with Status Pills

Current layout (Dashboard.cshtml):
```
[All] [Pending] [In Progress] [Completed] [Declined]     ← separate row
[🔍 Search input..........................] [Search]     ← separate row
```

Proposed layout:
```
[All] [Pending] [In Progress] [Completed] [Declined]  [🔍 Search...] [Search]
```
Single flex row: `d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3`

## 9. Booking Card Badge Fix

Current problem:
```
┌──────────────────────┐
│ Ma Ma       [Pending]│  ← name + badge flex row → collision on long names
```

Fix — stack layout:
```
┌──────────────────────┐
│ [New]                 │  ← absolute "New" stays
│ Ma Ma                 │  ← name on its own line
│ [Pending]  Repair     │  ← status badge + category below
│ 📅 20/06  🕐 AM      │
│ SC-00000001           │
```

Move status badge from `d-flex justify-content-between` header row into card body, placing it below the name on its own line or inline with category.

## 10. Files to Modify

| File | Change |
|------|--------|
| `ServiceCity.Core/Entities/Booking.cs` | Add `UserId?` + `User?` FK |
| `ServiceCity.Core/Entities/User.cs` | Add address field? (or use Booking address) |
| `ServiceCity.Data/AppDbContext.cs` | Add migration |
| `ServiceCity/Controllers/HomeController.cs` | Role-aware Index |
| `ServiceCity/Controllers/AuthController.cs` | Register action, role-aware sign-in redirect |
| `ServiceCity/Controllers/BookingController.cs` | Set UserId on create, MyBookings action |
| `ServiceCity/Controllers/AdminController.cs` | Customers action |
| `ServiceCity/Views/Home/Index.cshtml` | Guest vs customer conditional |
| `ServiceCity/Views/Shared/_Layout.cshtml` | Conditional nav links |
| `ServiceCity/Views/Admin/Dashboard.cshtml` | Inline search+pills, badge fix |
| `ServiceCity/Views/Booking/Lookup.cshtml` | Remove or redirect |
| `ServiceCity/Models/AdminDashboardViewModel.cs` | May add search placement |
| `ServiceCity/Models/BookingViewModel.cs` | May add UserId |
| `ServiceCity/wwwroot/css/site.css` | Badge overlap fix CSS |
| New: `ServiceCity/Views/Auth/Register.cshtml` | Registration form |
| New: `ServiceCity/Views/Booking/MyBookings.cshtml` | Customer's own bookings |
| New: `ServiceCity/Views/Admin/Customers.cshtml` | Admin customers list |
| New: `ServiceCity/Models/CustomerViewModel.cs` | Customer list model |

## 11. Risks & Edge Cases

| Risk | Mitigation |
|------|------------|
| Existing bookings have null UserId | Nullable FK — admin still sees them. Customer can't claim old bookings (acceptable for v1.1). |
| Admin could accidentally register as customer | Register action always sets `IsAdmin=false`. Admin created only via Setup. |
| SignIn redirect for admin breaks existing flow | Check `IsAdmin` claim → redirect appropriately. Admin still goes to Dashboard. |
| Phone lookup removed — what if customer forgets reference? | "My Bookings" shows all their bookings. No reference number needed. |
