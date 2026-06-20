---
phase: 02-auth-session
reviewed: 2026-06-19T00:00:00Z
depth: standard
files_reviewed: 15
files_reviewed_list:
  - ServiceCity.Core/Entities/User.cs
  - ServiceCity.Data/Configurations/UserConfiguration.cs
  - ServiceCity.Data/Migrations/20260619180142_AddAuthFields.Designer.cs
  - ServiceCity.Data/Migrations/20260619180142_AddAuthFields.cs
  - ServiceCity.Data/Migrations/AppDbContextModelSnapshot.cs
  - ServiceCity/Controllers/AdminController.cs
  - ServiceCity/Controllers/AuthController.cs
  - ServiceCity/Models/SetupViewModel.cs
  - ServiceCity/Models/SignInViewModel.cs
  - ServiceCity/Program.cs
  - ServiceCity/ServiceCity.csproj
  - ServiceCity/Views/Admin/Dashboard.cshtml
  - ServiceCity/Views/Auth/Setup.cshtml
  - ServiceCity/Views/Auth/SignIn.cshtml
  - ServiceCity/Views/Shared/_Layout.cshtml
findings:
  critical: 4
  warning: 6
  info: 4
  total: 14
status: issues_found
---

# Phase 02: Code Review Report

**Reviewed:** 2026-06-19
**Depth:** standard
**Files Reviewed:** 15
**Status:** issues_found

## Summary

Reviewed the auth-session phase implementation: User entity with auth fields, EF Core configuration and migration, AuthController (Setup/SignIn/SignOut), AdminController, view models, Razor views, and Program.cs cookie auth setup. The implementation is directionally sound but contains four critical issues: a broken sign-out form (missing antiforgery token causes 400 errors), a privilege escalation bug (all users get "Admin" role claim regardless of `IsAdmin` flag), a race condition allowing duplicate admin accounts during setup, and a silent TempData success message that is never displayed.

## Critical Issues

### CR-01: Sign-out form missing antiforgery token -- all sign-out attempts fail with 400

**File:** `ServiceCity/Views/Shared/_Layout.cshtml:37-38`
**Issue:** The sign-out form performs a POST to `/Auth/SignOut` but does not include `@Html.AntiForgeryToken()`. ASP.NET Core validates antiforgery tokens on all POST requests by default (via `AddControllersWithViews()` in Program.cs). The `AuthController.SignOut` action is decorated with `[HttpPost]` and does not have `[IgnoreAntiforgeryToken]`. Every sign-out attempt will be rejected with a 400 Bad Request, making it impossible for users to sign out.
**Fix:**
```html
<form asp-controller="Auth" asp-action="SignOut" method="post" class="d-inline">
    @Html.AntiForgeryToken()
    <button type="submit" class="btn nav-link text-dark border-0 bg-transparent">Sign Out</button>
</form>
```

### CR-02: Hardcoded "Admin" role claim grants admin privileges to any authenticated user

**File:** `ServiceCity/Controllers/AuthController.cs:105`
**Issue:** The SignIn method always adds `new(ClaimTypes.Role, "Admin")` to the claims identity regardless of the user's `IsAdmin` property. If a non-admin user account is ever created (e.g., via a future booking registration flow), that user would receive an admin role claim upon sign-in. This is a privilege escalation vulnerability.
**Fix:**
```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Name, user.Username!),
};
if (user.IsAdmin)
{
    claims.Add(new(ClaimTypes.Role, "Admin"));
}
// Or, only allow admin users to sign in:
// if (!user.IsAdmin) { ModelState.AddModelError(...); return View(model); }
```

### CR-03: TOCTOU race condition allows creation of multiple admin accounts

**File:** `ServiceCity/Controllers/AuthController.cs:27-28, 68-69`
**Issue:** The Setup action checks whether an admin exists (`db.Users.AnyAsync(u => u.IsAdmin)`) and then inserts a new admin user in a separate database operation. There is no database-level unique constraint on `IsAdmin == true`, and no transaction wrapping these operations. Two concurrent requests can both pass the `hasAdmin` check and both insert an admin, resulting in multiple admin accounts. The existing index on `PhoneNumber` is explicitly non-unique (`IsUnique(false)` in `UserConfiguration.cs:31-32`), so even that offers no protection.
**Fix:** Wrap the check and insert in a serializable transaction, or add a database-level constraint. The simplest approach is a filtered unique index:
```sql
CREATE UNIQUE INDEX IX_Users_SingleAdmin ON "Users" ("IsAdmin") WHERE "IsAdmin" = true;
```
Then catch the unique constraint exception in the controller. Alternatively, use an upsert or perform the check inside a transaction with `Serializable` isolation level.

### CR-04: TempData SuccessMessage from Setup never displayed to the user

**File:** `ServiceCity/Views/Auth/SignIn.cshtml`
**Issue:** After a successful admin account setup, `AuthController.Setup` sets `TempData["SuccessMessage"] = "Admin account created. Please sign in."` and redirects to `SignIn`. However, `SignIn.cshtml` has no code to render this TempData value. The success message is silently discarded, providing no feedback to the user that account creation was successful.
**Fix:** Add a TempData display block near the top of the form area in `SignIn.cshtml`:
```html
@if (TempData["SuccessMessage"] is string successMessage)
{
    <div class="alert alert-success">@successMessage</div>
}
```

## Warnings

### WR-01: No unique index on Username -- duplicate usernames produce unpredictable sign-in behavior

**File:** `ServiceCity.Data/Configurations/UserConfiguration.cs:26-27`
**Issue:** The `Username` property has `HasMaxLength(200)` but no unique index. If duplicate usernames exist, `FirstOrDefaultAsync(u => u.Username == model.Username)` in `AuthController.cs:86` returns whichever row PostgreSQL encounters first, which is non-deterministic. A user might sign in as a different user's account. The `PasswordHash` would still verify against the correct row since each user has their own hash, but a user with a duplicated username may never be able to sign into their own account.
**Fix:**
```csharp
builder.HasIndex(u => u.Username)
    .IsUnique();
```

### WR-02: `CookieSecurePolicy.SameAsRequest` weakens cookie security behind reverse proxies

**File:** `ServiceCity/Program.cs:20`
**Issue:** `CookieSecurePolicy.SameAsRequest` only sets the `Secure` flag on the auth cookie when the incoming request uses HTTPS. In Docker-based deployments where SSL is terminated at a reverse proxy (nginx, Traefik, AWS ALB), the application receives plain HTTP from the proxy. The auth cookie will not have the `Secure` flag, making it vulnerable to man-in-the-middle interception on the proxy-to-app network segment. While the documented stack uses Docker, this becomes a production security concern.
**Fix:** Use `CookieSecurePolicy.Always` for production or read the policy from configuration:
```csharp
options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;
```

### WR-03: `null!` passed to `PasswordHasher.HashPassword` suppresses null safety for no reason

**File:** `ServiceCity/Controllers/AuthController.cs:60`
**Issue:** `hasher.HashPassword(null!, model.Password)` passes `null!` (null with the null-forgiving operator) as the user parameter. While the base `PasswordHasher<TUser>` implementation currently ignores the user parameter for hashing, this is an undocumented implementation detail. The `TUser` parameter exists for key derivation purposes (used in ASP.NET Core Identity's `UserManager`). If the implementation changes in a future .NET update to use the user parameter for key derivation, all previously stored password hashes would fail verification because they were generated with `null` but verification passes `user`.
**Fix:** Pass the actual user object (or at minimum a non-null placeholder):
```csharp
// Option A: cast during construction
var user = new User
{
    Username = model.Username,
    Name = model.Name ?? model.Username,
    PhoneNumber = model.PhoneNumber ?? "",
    PhoneNumberNormalized = normalizedPhone,
    IsAdmin = true,
    CreatedAt = DateTime.UtcNow
};
user.PasswordHash = hasher.HashPassword(user, model.Password);

// Option B: use the user object after setting PasswordHash (circular, but works)
var user = new User { ... };
// HashPassword only reads properties from the user; PasswordHash hasn't been set yet
user.PasswordHash = hasher.HashPassword(user, model.Password);
```

### WR-04: Empty string stored for optional phone number instead of NULL

**File:** `ServiceCity/Controllers/AuthController.cs:62`
**Issue:** `PhoneNumber = model.PhoneNumber ?? ""` stores an empty string when the user does not provide a phone number. The `PhoneNumber` column in the database is `NOT NULL` (see migration snapshot line 199), so storing `""` is syntactically valid, but it is semantically wrong. An empty string means "the user provided an empty phone number," not "the user did not provide a phone number." This creates ambiguity when the field is eventually used for lookups or display. The user entity itself allows `PhoneNumber` to be a non-null `string` but defaults to `string.Empty`, which is inconsistent with `PhoneNumberNormalized` being `string?`. If phone is optional, consider making both nullable at the entity and database level.
**Fix:** Either store NULL for absent phone numbers (requires making the entity property and column nullable), or ensure the VIEW requires a phone number so `""` never occurs. For now the model marks it as optional (`string?`), so the entity should reflect that.

### WR-05: No rate limiting on sign-in endpoint

**File:** `ServiceCity/Controllers/AuthController.cs:82`
**Issue:** The sign-in POST endpoint has no rate limiting. While the password hashing operation is computationally expensive (providing some natural defense), an attacker can still attempt many passwords. The CLAUDE.md calls out rate limiting as a consideration, and the project targets Myanmar where slow mobile networks mean even modest brute force attempts degrade service for legitimate users.
**Fix:** Add ASP.NET Core's built-in rate limiting middleware to Program.cs:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("SignIn", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromMinutes(1);
    });
});
```
Then add `[EnableRateLimiting("SignIn")]` to the SignIn POST action.

### WR-06: Potential NullReferenceException on `model.Password.Length` if ModelState is bypassed

**File:** `ServiceCity/Controllers/AuthController.cs:32`
**Issue:** `model.Password.Length` depends on `model.Password` being non-null. The `[Required]` attribute on `SetupViewModel.Password` and the `ModelState.IsValid` check at line 30 should prevent null values. However, if model binding were ever changed or if a custom model binder populates `null`, this line would throw a `NullReferenceException`. Defensive null checking is low cost and high value, especially at security boundaries.
**Fix:**
```csharp
if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
{
    ModelState.AddModelError("Password", "Password must be at least 6 characters.");
    return View(model);
}
```

## Info

### IN-01: Weak password minimum length of 6 characters

**File:** `ServiceCity/Controllers/AuthController.cs:32`
**Issue:** The minimum password length is 6, which is below industry standard (typically 8 or 12 for admin accounts). Since this is an admin-only authentication system, the password should be stronger.
**Fix:** Increase the minimum to 8 or 12 characters.

### IN-02: jQuery loaded in layout despite project policy against it

**File:** `ServiceCity/Views/Shared/_Layout.cshtml:64`
**Issue:** The layout references `jquery.min.js` and jQuery validation plugins. CLAUDE.md explicitly states "Do NOT use jQuery" and recommends vanilla JS or htmx. This adds unnecessary page weight for mobile users in Myanmar. Note: `_ValidationScriptsPartial.cshtml` also references `jquery-validation` and `jquery-validation-unobtrusive`.
**Fix:** Replace jQuery validation with Bootstrap 5's built-in validation or vanilla JS. For the navbar toggler, Bootstrap 5's JS bundle already handles the collapse behavior without jQuery.

### IN-03: `new` keyword on `SignOut` hides base class member

**File:** `ServiceCity/Controllers/AuthController.cs:116`
**Issue:** `public new async Task<IActionResult> SignOut()` uses the `new` keyword to hide `Controller.SignOut()` (inherited from `ControllerBase`). This is fragile -- if the base class signature ever changes, behavior is unpredictable. Naming the action differently would avoid the member hiding entirely.
**Fix:** Rename to `LogOut` or `SignOutUser` to avoid the base class conflict.

### IN-04: Null-forgiving operator on `user.Username!` masks nullable reality

**File:** `ServiceCity/Controllers/AuthController.cs:104`
**Issue:** `new(ClaimTypes.Name, user.Username!)` uses the null-forgiving operator to suppress the compiler warning about `user.Username` being `string?`. The database column is nullable (migration has `nullable: true`), and a user created with a method other than Setup could have a null Username. If that user signs in, `ClaimTypes.Name` would receive a null value, which is technically allowed by `Claim` but semantically broken.
**Fix:**
```csharp
new(ClaimTypes.Name, user.Username ?? ""),
```

---

_Reviewed: 2026-06-19_
_Reviewer: Claude (gsd-code-reviewer)_
_Depth: standard_
