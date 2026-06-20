---
status: issues_found
files_reviewed: 22
depth: standard
phase: cross-phase (01-03)
critical: 3
warning: 7
info: 4
total: 14
---

# Code Review — All Phases (Cross-Phase)

**Reviewed:** 2026-06-20
**Scope:** All application source files across Phases 01–03
**Depth:** Standard

---

## Critical

### CR-001: Hardcoded database credentials in source control

**Files:** `appsettings.json:10`, `docker-compose.yml:7,27`

Database password `REDACTED` is committed in plaintext in both `appsettings.json` and `docker-compose.yml`. Anyone with repo access has full database credentials.

**Fix:** Move connection strings to environment variables or user-secrets. Use `.env` file (gitignored) for Docker Compose. For development, use `dotnet user-secrets`. For production, use Docker secrets or environment injection.

---

### CR-002: Weak reference number generation — `new Random()` is not cryptographically secure

**File:** `BookingController.cs:131`

```csharp
var random = new Random();
```

`System.Random` is predictable — reference numbers can be guessed/brute-forced, allowing booking data enumeration via `/Booking/Status?referenceNumber=SC-XXXXXXXX`.

**Fix:** Use `RandomNumberGenerator.GetInt32()` for cryptographically secure random generation:
```csharp
using System.Security.Cryptography;
// ...
var suffix = new string(Enumerable.Range(0, 8)
    .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
    .ToArray());
```

---

### CR-003: Missing `[ValidateAntiForgeryToken]` on admin Accept/Decline forms

**File:** `Views/Admin/Details.cshtml:29-42`

The Accept and Decline forms in the admin details view do not include `@Html.AntiForgeryToken()`. The controller actions have `[ValidateAntiForgeryToken]`, so these forms will **always fail** with a 400 error on submission.

**Fix:** Add `@Html.AntiForgeryToken()` inside both forms:
```html
<form asp-action="Accept" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" name="id" value="@Model.Id" />
    ...
</form>
```

Or use `asp-antiforgery="true"` on the form tag helper (it's the default for `form` tag helpers, but these forms use `asp-action` which should auto-generate it — verify the generated HTML).

---

## Warning

### WR-001: No booking status transition validation

**File:** `AdminController.cs:40-69`

Admin actions only check `Status != Pending` for Accept/Decline, but there's no validation for InProgress/Completed transitions. The controller also lacks actions for marking bookings InProgress or Completed — only Accept and Decline are implemented.

**Impact:** Phase 05 (Admin Actions) will need status transition logic. Current code allows accepting an already-accepted booking if the check is removed later.

**Fix:** Implement a status transition map:
```csharp
private static readonly Dictionary<BookingStatus, BookingStatus[]> ValidTransitions = new()
{
    { BookingStatus.Pending, new[] { BookingStatus.Accepted, BookingStatus.Declined } },
    { BookingStatus.Accepted, new[] { BookingStatus.InProgress, BookingStatus.Completed } },
    // ...
};
```

---

### WR-002: Raw phone stored instead of normalized in Booking.CustomerPhone

**File:** `BookingController.cs:60`

```csharp
CustomerPhone = model.CustomerPhone,  // raw user input
CustomerPhoneNormalized = normalized,  // E.164 format
```

The raw input is stored alongside the normalized version. This is inconsistent with the User entity pattern where `PhoneNumber` stores raw but `PhoneNumberNormalized` stores E164. While not a bug, it means the displayed phone number may be inconsistently formatted.

**Fix:** Store the normalized phone as `CustomerPhone` too, or accept this as intentional (showing user's original input).

---

### WR-003: Cookie `SecurePolicy` set to `SameAsRequest` — insecure for production

**File:** `Program.cs:21`

```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
```

In production over HTTP (e.g., behind a reverse proxy without TLS termination), the auth cookie will be sent without the Secure flag, making it vulnerable to interception.

**Fix:** Use `CookieSecurePolicy.Always` in production:
```csharp
options.Cookie.SecurePolicy = app.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;
```

---

### WR-004: `SignOut` uses `new` keyword to hide base `Controller.SignOut`

**File:** `AuthController.cs:122`

```csharp
public new async Task<IActionResult> SignOut()
```

The `new` keyword hides `Controller.SignOut()`. This works but is a code smell — if someone calls `base.SignOut()` expecting the built-in behavior, it won't match.

**Fix:** Rename to `SignOutPost` or `LogOut` and update the route/form action. Or accept the current pattern since the `[HttpPost]` attribute disambiguates.

---

### WR-005: No concurrency control on admin booking actions

**File:** `AdminController.cs:40-69`

Two admins could accept/decline the same booking simultaneously. The `booking.Status != BookingStatus.Pending` check is not atomic — race condition between `FindAsync` and `SaveChangesAsync`.

**Fix:** Use optimistic concurrency (row version) or a conditional update:
```csharp
var affected = await db.Bookings
    .Where(b => b.Id == id && b.Status == BookingStatus.Pending)
    .ExecuteUpdateAsync(s => s
        .SetProperty(b => b.Status, BookingStatus.Accepted)
        .SetProperty(b => b.UpdatedAt, DateTime.UtcNow));
```

---

### WR-006: `PreferredDate` default is server-local time, not UTC

**File:** `BookingViewModel.cs:29`

```csharp
public DateTime PreferredDate { get; set; } = DateTime.Today.AddDays(1);
```

`DateTime.Today` uses server-local time. The controller then does `DateTime.SpecifyKind(model.PreferredDate, DateTimeKind.Utc)` which reinterprets the same value as UTC without converting — a date picked as "June 21 local" becomes "June 21 UTC" which may be a different day.

**Fix:** Use UTC consistently or store dates as `DateOnly` (no timezone ambiguity):
```csharp
public DateOnly PreferredDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
```

---

### WR-007: HTMX loaded from unpkg CDN — no integrity hash

**File:** `Views/Shared/_Layout.cshtml:68`

```html
<script src="https://unpkg.com/htmx.org@2.0.0"></script>
```

No `integrity` attribute — supply chain attack vector if unpkg is compromised. Also adds a network dependency for Myanmar users on slow connections.

**Fix:** Add subresource integrity:
```html
<script src="https://unpkg.com/htmx.org@2.0.0" integrity="sha384-..." crossorigin="anonymous"></script>
```
Or self-host the file in `wwwroot/lib/`.

---

## Info

### IN-001: `ValidateAndNormalizePhone` duplicated between AuthController and BookingController

**Files:** `AuthController.cs:129-146`, `BookingController.cs:146-163`

Identical private methods. Extract to a shared service or static utility class.

---

### IN-002: `ToLocalTime()` in Details.cshtml without timezone context

**File:** `Views/Admin/Details.cshtml:18`

```csharp
@Model.CreatedAt.ToLocalTime()
```

"Local" depends on the server's timezone, not the user's. For a Myanmar-focused app, consider using Myanmar Time (UTC+6:30) explicitly.

---

### IN-003: No `[Authorize(Roles = "Admin")]` — uses generic `[Authorize]`

**File:** `AdminController.cs:10`

Any authenticated user (not just admins) can access the admin dashboard. Currently only admins can sign in, but this should be explicitly role-restricted for defense-in-depth.

**Fix:**
```csharp
[Authorize(Roles = "Admin")]
```

---

### IN-004: Rate limiter is global, not per-phone

**File:** `Program.cs:28-37`

The `BookingSubmission` rate limiter uses a fixed window by IP, not by phone number. A user with multiple phone numbers can bypass the limit; multiple users behind the same NAT share the limit.

---

## Summary

| Severity | Count | Key Theme |
|----------|-------|-----------|
| Critical | 3 | Credentials in source, weak RNG, broken CSRF |
| Warning | 7 | Missing status transitions, concurrency, date handling |
| Info | 4 | Code duplication, role authorization, rate limiting |

**Priority fixes before Phase 03 goes live:**
1. CR-003 (broken Accept/Decline forms — feature won't work)
2. CR-001 (credentials in source — security)
3. CR-002 (weak reference numbers — enumeration risk)
