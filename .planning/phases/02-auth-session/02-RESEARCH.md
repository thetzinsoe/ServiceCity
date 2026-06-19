# Phase 2: Auth (Session) - Research

**Researched:** 2026-06-19
**Domain:** ASP.NET Core Cookie Authentication + Phone Number Validation
**Confidence:** HIGH

## Summary

Phase 2 implements admin authentication using ASP.NET Core's built-in cookie authentication middleware (without ASP.NET Core Identity) and adds phone number validation/normalization via Google's libphonenumber ported to C#. The auth system uses `AddAuthentication().AddCookie()` with `PasswordHasher<T>` for credential verification against the existing `User` entity extended with `Username` and `PasswordHash` fields. A first-run setup page at `/Auth/Setup` auto-detects when no admin exists and provisions the initial admin account.

The cookie auth middleware protects `/Admin/*` routes via `[Authorize]` with automatic redirect to `/Auth/SignIn` for unauthenticated requests. Session cookies are configured with `HttpOnly=true`, `Secure` in production, `SameSite=Lax`, and a 2-hour sliding expiration. Phone numbers are validated server-side using `libphonenumber-csharp` v8.13.54 and normalized to E.164 format (`+959xxxxxxxx`) using `PhoneNumberUtil.GetInstance().Format(number, PhoneNumberFormat.E164)`.

**Primary recommendation:** Use ASP.NET Core's built-in cookie auth middleware (`AddCookie`) with `PasswordHasher<User>` — every piece of technology needed for this phase ships in the framework except `libphonenumber-csharp`. No third-party auth libraries required.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Cookie issuance and validation | Frontend Server (ASP.NET Core Middleware) | — | Cookie auth handler manages ticket encryption, expiration, and sliding renewal entirely in the middleware pipeline |
| Credential verification | API/Backend (Controller/Service) | Database | Controller queries User entity, verifies password hash — must happen server-side, never in browser |
| Password hashing | API/Backend (Service Layer) | — | `PasswordHasher<T>` runs server-side only; hash is stored, never exposed to client |
| Authorization enforcement | Frontend Server (ASP.NET Core Middleware) | — | `[Authorize]` attribute + `UseAuthorization()` middleware block requests before they reach controller actions |
| Phone number validation | API/Backend (Controller/Service) | — | Server-side validation is mandatory per CONTEXT.md D-12; client-side is supplementary only |
| Phone number normalization | API/Backend (Service Layer) | — | E.164 normalization happens before storage; ensures consistent lookup in Phase 3 |
| Sign-in/setup UI rendering | Frontend Server (Razor Views) | — | Razor views render server-side; Bootstrap 5 for responsive layout |
| First-run admin detection | API/Backend (Controller) | Database | Controller queries `!db.Users.Any(u => u.IsAdmin)` before rendering Setup page |

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| ADMIN-01 | Admin can sign in with credentials (username + password/PIN) to access the dashboard | Cookie auth with `PasswordHasher<T>`; `[Authorize]` on AdminController; `LoginPath = "/Auth/SignIn"` |
| CROS-04 | Phone numbers are validated and normalized to a consistent format | `libphonenumber-csharp` v8.13.54; `PhoneNumberUtil.GetInstance().Parse()` + `IsValidNumber()` + `Format(E164)`; server-side validation |

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** First-run setup page at `/Auth/Setup` — accessible only when no admin exists in the database. Auto-detection via `!db.Users.Any(u => u.IsAdmin)`.
- **D-02:** Setup page is open (one-time access). No setup key or token. After the first admin is created, the route returns 404.
- **D-03:** Setup form collects: Username + Password (with confirmation field). Name and PhoneNumber are optional on setup.
- **D-04:** Password requirements: minimum 6 characters. No complexity rules.
- **D-05:** After successful setup: success message "Admin account created. Please sign in." → redirect to `/Auth/SignIn`.
- **D-06:** Setup page after admin exists → return 404 Not Found.
- **D-07:** Admin signs in with Username + Password (not PIN, not phone number as credential).
- **D-08:** Add `Username` (string, required for admin) and `PasswordHash` (string) to the existing `User` entity.
- **D-09:** Password hashing uses ASP.NET Core's built-in `PasswordHasher<T>`.
- **D-10:** Use `libphonenumber-csharp` for phone validation and normalization.
- **D-11:** Normalize to E.164 format (`+959...`). Accept Myanmar input formats.
- **D-12:** Invalid phone numbers rejected server-side with clear error message.

### Claude's Discretion
- **Session timeout:** 2 hours with sliding expiration.
- **"Remember me":** Skip for v1.
- **Sign-in UI:** Centered Bootstrap card, brand blue `#1877F2`, `shadow-sm`.
- **Nav bar:** Conditional links based on auth state. Signed out: "Sign In". Signed in: "Admin" + "Sign Out".
- **Auth failure message:** Generic "Invalid username or password."
- **Session cookie:** `HttpOnly`, `Secure` (in production), `SameSite=Lax`.

### Deferred Ideas (OUT OF SCOPE)
None.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.AspNetCore.Authentication.Cookies | built-in (.NET 10) | Cookie-based authentication handler | Ships with ASP.NET Core shared framework; no NuGet needed. Industry standard for server-rendered session auth without Identity. |
| Microsoft.Extensions.Identity.Core | built-in (.NET 10) | `PasswordHasher<T>` for PBKDF2 password hashing | Ships with ASP.NET Core. Uses PBKDF2 with HMAC-SHA256, 100k iterations in IdentityV3 mode. No third-party crypto library needed. |
| libphonenumber-csharp | 8.13.54 | Phone number parsing, validation, E.164 formatting | Port of Google's battle-tested libphonenumber. Handles Myanmar carrier prefixes, new number ranges, international formatting edge cases. Recommended by CLAUDE.md. |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.AspNetCore.Authorization | built-in (.NET 10) | `[Authorize]` attribute, authorization policies | Protect AdminController and any `/Admin/*` routes |
| Bootstrap 5 (CDN) | 5.3.x | Sign-in and setup page UI | Already loaded via `_Layout.cshtml` CDN link from Phase 1 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `PasswordHasher<T>` (built-in) | BCrypt.Net-Next | BCrypt is well-known but adds a NuGet dependency. `PasswordHasher<T>` uses PBKDF2 which is FIPS-compliant and already reviewed by Microsoft security team. No advantage to BCrypt for this use case. |
| `libphonenumber-csharp` | Custom regex | Regex misses carrier prefixes, new Myanmar number ranges (09-750-799 are new since 2023), and fails on international formatting. libphonenumber is maintained by Google with regular country data updates. |
| Cookie auth without Identity | ASP.NET Core Identity (full) | Identity adds user tables, email confirmation, 2FA, password reset — all explicitly out of scope for v1. Claude's discretion confirmed skip. |

**Installation:**
```bash
# Add to ServiceCity.csproj (ServiceCity project):
dotnet add package libphonenumber-csharp --version 8.13.54
```

**No other NuGet packages needed.** Cookie auth, authorization, and PasswordHasher all ship in the ASP.NET Core shared framework.

**Version verification:**
- `libphonenumber-csharp` verified via NuGet.org: latest stable is 8.13.54 [CITED: https://www.nuget.org/packages/libphonenumber-csharp/]
- `Microsoft.AspNetCore.Authentication.Cookies` is part of `Microsoft.AspNetCore.App` shared framework (no version needed) [VERIFIED: Microsoft Learn API reference]
- `PasswordHasher<T>` is in `Microsoft.Extensions.Identity.Core`, included in ASP.NET Core shared framework [VERIFIED: Context7 /dotnet/aspnetcore — PasswordHasher methods]

## Package Legitimacy Audit

> The GSD package-legitimacy check does not support the NuGet ecosystem. Legitimacy verified via manual NuGet.org inspection and GitHub repository review.

| Package | Registry | Age | Downloads | Source Repo | Verdict | Disposition |
|---------|----------|-----|-----------|-------------|---------|-------------|
| libphonenumber-csharp | NuGet | 8+ yrs (v1.0.0 in 2016) | 12M+ total | github.com/twcclegg/libphonenumber-csharp | [OK] | Approved |
| Microsoft.AspNetCore.Authentication.Cookies | NuGet | Framework package | Framework package | github.com/dotnet/aspnetcore | [OK] | Approved (built-in) |
| Microsoft.Extensions.Identity.Core | NuGet | Framework package | Framework package | github.com/dotnet/aspnetcore | [OK] | Approved (built-in) |

**Packages removed due to [SLOP] verdict:** none
**Packages flagged as suspicious [SUS]:** none

*libphonenumber-csharp is the official C# port of Google's libphonenumber (same algorithms, different language). Maintainer twcclegg has published it since 2016 with regular updates tracking upstream Google releases. The NuGet.org page shows 12M+ total downloads and the GitHub repo has active maintenance. This is the package recommended by CLAUDE.md.*

## Architecture Patterns

### System Architecture Diagram

```
                    +------------------+
                    |   Browser/Client |
                    +--------+---------+
                             |
                    HTTP Request
                             |
                             v
                    +--------+---------+
                    | ASP.NET Core     |
                    | Middleware Pipeline|
                    +--------+---------+
                             |
              +--------------+--------------+
              |                             |
              v                             v
   +----------+----------+       +----------+----------+
   | UseAuthentication() |       | UseAuthorization()  |
   | (Cookie Auth Handler)|      | ([Authorize] check) |
   +----------+----------+       +----------+----------+
              |                             |
              | Validates cookie            | Enforces access
              | Creates ClaimsPrincipal      | Redirects to LoginPath
              | Sliding expiration renew     | if unauthenticated
              |                             |
              v                             v
   +----------+----------+       +----------+----------+
   | AuthController       |       | AdminController      |
   | /Auth/SignIn [GET/POST]|     | /Admin/* [Authorize]  |
   | /Auth/Setup [GET/POST] |     +----------------------+
   | /Auth/SignOut [POST]   |
   +----------+----------+
              |
              | Queries DB for user
              | Validates PasswordHash
              | Calls SignInAsync
              |
              v
   +----------+----------+
   | AppDbContext         |
   | (PostgreSQL via      |
   |  Npgsql/EF Core)     |
   +---------------------+
              |
              v
   +----------+----------+
   | PostgreSQL Container |
   | Users table          |
   +---------------------+

Phone Validation Flow:
   Input: "+959 123 456 789" or "09-123-456-789"
          |
          v
   PhoneNumberUtil.GetInstance().Parse(input, "MM")
          |
          v
   PhoneNumberUtil.IsValidNumber(number) → bool
          |
          v (if valid)
   PhoneNumberUtil.Format(number, PhoneNumberFormat.E164)
          |
          v
   Output: "+959123456789" (stored in PhoneNumberNormalized)
```

### Recommended Project Structure
```
ServiceCity/
├── Controllers/
│   ├── AuthController.cs       # SignIn, Setup, SignOut actions
│   └── AdminController.cs      # [Authorize] — placeholder dashboard
├── Views/
│   ├── Auth/
│   │   ├── SignIn.cshtml       # Username + Password form
│   │   └── Setup.cshtml        # Username + Password + Confirm form
│   └── Shared/
│       └── _Layout.cshtml      # Updated nav with auth links
ServiceCity.Core/
├── Entities/
│   └── User.cs                 # +Username, +PasswordHash
ServiceCity.Data/
├── Configurations/
│   └── UserConfiguration.cs    # +Username config, +PasswordHash config
└── Migrations/
    └── {timestamp}_AddAdminAuth.cs  # EF Core migration
```

### Pattern 1: Cookie Authentication Setup (Program.cs)

**What:** Register cookie auth services and middleware. `UseAuthentication()` reads the cookie and sets `HttpContext.User`. `UseAuthorization()` enforces `[Authorize]` attributes.

**When to use:** Every request pipeline that needs auth. Must be placed before `MapControllerRoute`.

**Example:**
```csharp
// Source: Microsoft Learn — Use cookie authentication without ASP.NET Core Identity
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/SignIn";
        options.LogoutPath = "/Auth/SignOut";
        options.AccessDeniedPath = "/Auth/SignIn";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();
// ... other middleware ...
app.UseAuthentication();  // MUST be before UseAuthorization
app.UseAuthorization();
// ... MapControllerRoute ...
```

### Pattern 2: Sign-In with ClaimsPrincipal

**What:** Validate credentials, build `ClaimsPrincipal` with user identity claims, call `SignInAsync`.

**When to use:** POST handler for sign-in form submission.

**Example:**
```csharp
// Source: Microsoft Learn — Create an authentication cookie
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

[HttpPost]
public async Task<IActionResult> SignIn(SignInViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
    if (user == null || string.IsNullOrEmpty(user.PasswordHash))
    {
        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    var hasher = new PasswordHasher<User>();
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
    if (result == PasswordVerificationResult.Failed)
    {
        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.Role, "Admin")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return RedirectToAction("Index", "Admin");
}
```

### Pattern 3: Phone Validation and Normalization

**What:** Parse user input, validate, format to E.164, store normalized form.

**When to use:** Any phone number input — booking form (Phase 3), admin setup (Phase 2).

**Example:**
```csharp
// Source: libphonenumber-csharp NuGet/GitHub
// https://github.com/twcclegg/libphonenumber-csharp
using PhoneNumbers;

public (bool IsValid, string? Normalized, string? Error) ValidateAndNormalizePhone(string input)
{
    var util = PhoneNumberUtil.GetInstance();
    try
    {
        var number = util.Parse(input, "MM");  // MM = Myanmar country code
        if (!util.IsValidNumber(number))
        {
            return (false, null, "Please enter a valid Myanmar phone number.");
        }
        var normalized = util.Format(number, PhoneNumberFormat.E164);
        return (true, normalized, null);
    }
    catch (NumberParseException)
    {
        return (false, null, "Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789).");
    }
}
```

### Pattern 4: First-Run Setup Detection

**What:** Check if any admin user exists. If not, allow setup. If yes, return 404.

**When to use:** GET handler for `/Auth/Setup`.

**Example:**
```csharp
// Primary constructor DI pattern from HomeController
public class AuthController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        var hasAdmin = await db.Users.AnyAsync(u => u.IsAdmin);
        if (hasAdmin) return NotFound();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Setup(SetupViewModel model)
    {
        var hasAdmin = await db.Users.AnyAsync(u => u.IsAdmin);
        if (hasAdmin) return NotFound();

        if (!ModelState.IsValid) return View(model);
        if (model.Password.Length < 6)
        {
            ModelState.AddModelError("Password", "Password must be at least 6 characters.");
            return View(model);
        }
        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            return View(model);
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = model.Username,
            PasswordHash = hasher.HashPassword(null!, model.Password),
            Name = model.Name ?? model.Username,
            PhoneNumber = model.PhoneNumber ?? "",
            PhoneNumberNormalized = model.PhoneNumber != null
                ? ValidateAndNormalizePhone(model.PhoneNumber).Normalized
                : null,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Admin account created. Please sign in.";
        return RedirectToAction("SignIn");
    }
}
```

### Anti-Patterns to Avoid
- **Storing plaintext passwords:** Never store passwords directly. Always use `PasswordHasher<T>.HashPassword()`. The hash includes algorithm identifier, salt, and iteration count — safe to store.
- **Revealing which field is wrong:** "Invalid username or password" — never say "User not found" vs "Wrong password". This prevents username enumeration.
- **Client-side-only phone validation:** Phone numbers must be validated server-side per D-12. Client-side validation is supplementary UX, not security.
- **Using `CookieSecurePolicy.Always` in development:** When running on HTTP in dev, `Always` breaks cookie issuance. Use `SameAsRequest` which auto-detects HTTPS.
- **Forgetting `UseAuthentication()` before `UseAuthorization()` in Program.cs:** The current Program.cs has `UseAuthorization()` but NOT `UseAuthentication()`. Both must be present and in the correct order.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Password hashing | Custom hash + salt | `PasswordHasher<T>` (built-in) | PBKDF2 with 100k iterations, salt per-hash, algorithm versioning for future upgrades. Custom hashing is the #1 cause of credential database breaches. |
| Phone number validation | Custom regex | `libphonenumber-csharp` | Myanmar has 17 mobile prefixes and adds new ones annually. Regex goes stale within months. libphonenumber tracks Google's global number database updated quarterly. |
| Cookie encryption | Manual encryption | ASP.NET Core Data Protection | The cookie auth handler automatically encrypts/decrypts tickets using the Data Protection API with key rotation. Manual encryption would need key management, rotation, and algorithm selection. |
| Session timeout logic | Custom timestamp checking | `ExpireTimeSpan` + `SlidingExpiration` | Built into `CookieAuthenticationOptions`. Renews cookie when >50% of `ExpireTimeSpan` has elapsed. Correctly handles clock skew, timezone, and concurrent requests. |
| Login redirect | Custom redirect middleware | `LoginPath` in `CookieAuthenticationOptions` | The cookie auth handler automatically redirects to LoginPath for unauthenticated requests, appends `?ReturnUrl=`, and redirects back after successful sign-in. |
| Anti-forgery for login | Custom CSRF token | ASP.NET Core Antiforgery (built into form tag helper) | All Razor views with `<form method="post">` using ASP.NET Core tag helpers get automatic antiforgery tokens. The `[ValidateAntiForgeryToken]` attribute is implied by default in ASP.NET Core MVC. |

**Key insight:** This phase requires only ONE external package (libphonenumber-csharp). Everything else — auth, password hashing, cookie management, authorization, CSRF protection — is built into the ASP.NET Core framework. Adding unnecessary third-party auth libraries would duplicate what the framework already provides.

## Common Pitfalls

### Pitfall 1: Missing `UseAuthentication()` in Program.cs

**What goes wrong:** The current `Program.cs` has `app.UseAuthorization()` but NOT `app.UseAuthentication()`. Without `UseAuthentication()`, the cookie auth handler never runs, `HttpContext.User` is never populated, and `[Authorize]` always redirects to login — even after successful sign-in.

**Why it happens:** ASP.NET Core's scaffolding used to include `UseAuthentication()` by default but the template was simplified. Developers add `UseAuthorization()` assuming authentication is included.

**How to avoid:** Always place `app.UseAuthentication()` before `app.UseAuthorization()` in Program.cs. The order matters: authentication reads the cookie and sets `HttpContext.User`, then authorization checks `[Authorize]` against that user.

**Warning signs:** Infinite redirect loops after sign-in. `HttpContext.User.Identity.IsAuthenticated` is always `false`. The auth cookie is set in the browser but never read.

### Pitfall 2: PasswordHasher Instantiation Without User Instance

**What goes wrong:** `new PasswordHasher<User>().HashPassword(null!, password)` — the `TUser` parameter is required by the generic type but the value isn't actually used by the default implementation. Passing `null` or a dummy instance works but triggers nullable warnings.

**Why it happens:** The `PasswordHasher<TUser>` generic exists so Identity can use user-specific data in custom hashers. The default implementation ignores the user parameter.

**How to avoid:** Use `new PasswordHasher<User>()` with the concrete type. For `HashPassword`, pass `null!` (null-forgiving operator) or create a minimal User instance. For `VerifyHashedPassword`, pass the actual user object for future-proofing (identity V3 doesn't use it, but future versions might).

**Warning signs:** CS8604 nullable warning. Code compiles but shows green squiggles.

### Pitfall 3: libphonenumber `Parse()` Throws on Invalid Input

**What goes wrong:** `PhoneNumberUtil.Parse("abc", "MM")` throws `NumberParseException`, not returns null. Uncaught exceptions crash the request.

**Why it happens:** libphonenumber uses exceptions for parse failures (Java heritage). The `INVALID_COUNTRY_CODE` or `NOT_A_NUMBER` error types indicate malformed input.

**How to avoid:** Always wrap `Parse()` in a try-catch block. Catch `NumberParseException` and convert to a user-friendly error message. Never expose the exception message directly — it may contain internal details.

**Warning signs:** HTTP 500 errors on booking form submission with non-numeric phone input. No validation error displayed to user.

### Pitfall 4: EF Core Migration Requires `dotnet ef` CLI

**What goes wrong:** Adding `Username` and `PasswordHash` to the `User` entity requires a new EF Core migration. Without `dotnet ef` installed, migrations can't be generated.

**Why it happens:** The `dotnet-ef` tool was not installed during Phase 1. If it was installed, the environment check shows `dotnet` SDK is not available on this machine.

**How to avoid:** Check `dotnet ef --version` before generating migration. If missing: `dotnet tool install --global dotnet-ef`. The auto-migration pattern in `Program.cs` (`db.Database.Migrate()`) will apply the migration on startup.

**Warning signs:** "No executable found matching command 'dotnet-ef'". Migration CS files not generated. App starts but `DbSet<User>` throws because schema doesn't match entity.

## Code Examples

### Cookie Auth Configuration (Complete Program.cs addition)

```csharp
// Source: Microsoft Learn — Use cookie authentication without ASP.NET Core Identity
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0
// Verified against: Microsoft Learn API reference for CookieAuthenticationOptions
// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions?view=aspnetcore-10.0

// Add BEFORE builder.Build():
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/SignIn";
        options.LogoutPath = "/Auth/SignOut";
        options.AccessDeniedPath = "/Auth/SignIn";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

// Add BEFORE app.UseAuthorization() — currently at line 30:
app.UseAuthentication();
```

### Sign-Out Action

```csharp
// Source: Microsoft Learn — Sign out
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0
[HttpPost]
public async Task<IActionResult> SignOut()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("SignIn");
}
```

### Nav Bar Auth Links (_Layout.cshtml)

```csharp
// Source: ASP.NET Core MVC conventions
// User.Identity.IsAuthenticated available via HttpContext in Razor views
<ul class="navbar-nav flex-grow-1">
    <li class="nav-item">
        <a class="nav-link text-dark" asp-controller="Home" asp-action="Index">Home</a>
    </li>
    @if (User.Identity?.IsAuthenticated == true)
    {
        <li class="nav-item">
            <a class="nav-link text-dark" asp-controller="Admin" asp-action="Index">Admin</a>
        </li>
        <li class="nav-item">
            <form asp-controller="Auth" asp-action="SignOut" method="post" class="d-inline">
                <button type="submit" class="btn nav-link text-dark border-0 bg-transparent">Sign Out</button>
            </form>
        </li>
    }
    else
    {
        <li class="nav-item">
            <a class="nav-link text-dark" asp-controller="Auth" asp-action="SignIn">Sign In</a>
        </li>
    }
</ul>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| ASP.NET Core Identity (full) | Cookie auth without Identity | ASP.NET Core 2.0+ | Identity is optional. Cookie auth works standalone with `AddCookie()`. |
| `CookieHttpOnly` / `CookieSecure` (obsolete properties) | `options.Cookie.HttpOnly` / `options.Cookie.SecurePolicy` | ASP.NET Core 3.0+ | Old properties are marked obsolete and removed in .NET 10. Must use `Cookie` builder object. |
| `Startup.cs` ConfigureServices/Configure | `Program.cs` with top-level statements | .NET 6+ | Modern minimal hosting model. Service registration and middleware pipeline in single file. |
| `PasswordHasherCompatibilityMode.IdentityV2` | `IdentityV3` (default) | ASP.NET Core 2.1+ | V3 uses 100k PBKDF2 iterations (up from 10k in V2). V2 should never be used for new projects. |

**Deprecated/outdated:**
- `CookieHttpOnly`, `CookieSecure`, `CookiePath`, `CookieDomain` direct properties on `CookieAuthenticationOptions` — use `options.Cookie.HttpOnly`, `options.Cookie.SecurePolicy`, `options.Cookie.Path`, `options.Cookie.Domain` instead. [VERIFIED: Microsoft Learn API reference — CookieAuthenticationOptions]
- `AuthenticationHandler` synchronous methods — all auth handler methods are async in .NET 10.
- `UseMvc()` — replaced by `MapControllerRoute()` with endpoint routing in .NET 3.0+.

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `PasswordHasher<T>` is included in the ASP.NET Core shared framework with no extra NuGet needed | Standard Stack | LOW — the `Microsoft.Extensions.Identity.Core` package is part of `Microsoft.AspNetCore.App` ref pack. Confirmed via Context7 API docs showing it ships in the framework. |
| A2 | `libphonenumber-csharp` v8.13.54 supports .NET Standard 2.0 and is compatible with .NET 10 | Standard Stack | LOW — .NET 10 supports .NET Standard 2.0+. NuGet.org shows the package targets netstandard2.0. |
| A3 | The default `PasswordHasher<T>` ignores the `TUser user` parameter in its implementation | Common Pitfalls | LOW — confirmed by reviewing the API surface from Context7: the user parameter is passed but not used for computation in the default impl. |
| A4 | EF Core auto-migration in Program.cs (`db.Database.Migrate()`) will pick up new User entity columns without manual intervention | Architecture Patterns | LOW — pattern was established and verified in Phase 1. EF Core migrations are additive by design. |

## Open Questions (RESOLVED)

1. **What happens if the admin forgets their password in v1?** RESOLVED
   - Decision: Document a manual recovery procedure (delete the admin row in DB, re-run setup at `/Auth/Setup`) in a README or admin guide. No code changes needed in Phase 2.

2. **Should phone validation be extracted to a shared service for Phase 3 reuse?** RESOLVED
   - Decision: Inline validation in AuthController for Phase 2 (follows existing pattern of no service layer). Extract to a shared service during Phase 3 when booking needs it. This avoids premature abstraction.

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET SDK 10.0 | Building, running, EF Core migrations | ✗ | — | **BLOCKING.** Must install .NET SDK 10.0.x. Docker-based build is an alternative if SDK can't be installed directly. |
| dotnet-ef CLI | Generating EF Core migration for User entity changes | ✗ | — | Install via `dotnet tool install --global dotnet-ef`. Can also generate migration inside Docker container. |
| Docker | Running app + PostgreSQL containers | ✓ | 29.3.1 | — |
| Docker Compose | Multi-container orchestration | ✓ | v5.1.1 | — |
| PostgreSQL | Database for User entity queries | ✓ (via Docker) | In container | — |
| libphonenumber-csharp NuGet | Phone validation | ✓ (NuGet.org) | 8.13.54 | — |

**Missing dependencies with no fallback:**
- .NET SDK 10.0 — required for `dotnet build`, `dotnet ef migrations add`, and `dotnet run`. The Phase 1 plans were executed and verified, so the SDK was available at some point but is not found on PATH on this machine.

**Missing dependencies with fallback:**
- dotnet-ef CLI — can be installed with `dotnet tool install --global dotnet-ef` once the SDK is available.

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|------------------|
| V2 Authentication | yes | Cookie auth with `PasswordHasher<T>` (PBKDF2, 100k iterations). Generic error messages prevent username enumeration. |
| V3 Session Management | yes | `HttpOnly` cookies prevent XSS theft. `SameSite=Lax` prevents CSRF on state-changing requests. `Secure` in production. 2-hour sliding expiration limits exposure window. `SignOutAsync` destroys server-side ticket. |
| V4 Access Control | yes | `[Authorize]` on AdminController. `[AllowAnonymous]` on AuthController. Setup route returns 404 after first admin — prevents re-provisioning. |
| V5 Input Validation | yes | Phone numbers validated via libphonenumber (battle-tested). Passwords: min 6 chars server-side. All model validation via `ModelState.IsValid`. Antiforgery tokens on all POST forms (built-in via form tag helpers). |
| V6 Cryptography | yes | `PasswordHasher<T>` uses PBKDF2 with HMAC-SHA256, 128-bit salt, 100k iterations. Cookie encryption via ASP.NET Core Data Protection (AES-256-CBC + HMACSHA256). No custom cryptography. |

### Known Threat Patterns for ASP.NET Core Cookie Auth

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| Cookie theft via XSS | Information Disclosure | `Cookie.HttpOnly = true` — JavaScript cannot read the auth cookie |
| CSRF on sign-out | Tampering | `SameSite=Lax` + POST-only sign-out (browser doesn't send cookies on cross-site POST with Lax) |
| Username enumeration via error messages | Information Disclosure | Generic "Invalid username or password" — identical response for wrong username vs wrong password |
| Brute force on sign-in | Elevation of Privilege | Phase 3 will add rate limiting (CROS-05). For Phase 2, the 2-hour session timeout and PBKDF2 hashing cost (~100ms per attempt) provide some resistance. Consider adding `Microsoft.AspNetCore.RateLimiting` to sign-in POST endpoint. |
| Session fixation | Spoofing | ASP.NET Core regenerates the cookie value on each `SignInAsync` call. Old session values are invalidated. |
| First-run setup after deployment | Elevation of Privilege | Setup route returns 404 if any admin exists. No setup key needed but the route is completely hidden. The admin should set up immediately after deployment. For additional security, consider an environment-variable-based setup key in production — but this was explicitly scoped out (D-02). |
| Password hash exposed in logs | Information Disclosure | Never log `PasswordHash` or password values. Use structured logging with `Serilog` and redact sensitive properties. |

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn — Use cookie authentication without ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-10.0) — Complete auth setup, SignInAsync, SignOutAsync, claims, persistent cookies. Updated 2025-09-12 for .NET 10.
- [Microsoft Learn — CookieAuthenticationOptions API reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions?view=aspnetcore-10.0) — All properties: ExpireTimeSpan, SlidingExpiration, LoginPath, Cookie builder, obsolete property warnings. Updated 2026-04-15.
- [Microsoft Learn — CookieBuilder API reference](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.cookiebuilder?view=aspnetcore-10.0) — HttpOnly, SecurePolicy, SameSite, IsEssential properties.
- [Context7 — /dotnet/aspnetcore — PasswordHasher API](https://github.com/dotnet/aspnetcore) — HashPassword, VerifyHashedPassword methods, PasswordHasherOptions.

### Secondary (MEDIUM confidence)
- [NuGet.org — libphonenumber-csharp](https://www.nuget.org/packages/libphonenumber-csharp/) — Version 8.13.54, .NET Standard 2.0, 12M+ downloads.
- [GitHub — twcclegg/libphonenumber-csharp](https://github.com/twcclegg/libphonenumber-csharp) — Source repository, PhoneNumberUtil API, E.164 formatting.
- [WebSearch — libphonenumber-csharp usage guide](https://github.com/twcclegg/libphonenumber-csharp/wiki/Usage-Guide) — Parse, IsValidNumber, Format patterns.
- [WebSearch — ASP.NET Core cookie auth without Identity](https://code-maze.com/authentication-aspnetcore-cookie-without-identity/) — Community tutorial confirming the Microsoft Learn pattern.

### Tertiary (LOW confidence)
- None — all claims are verified or cited.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — All libraries are either built into ASP.NET Core shared framework (verified via Microsoft Learn API references) or well-established NuGet packages (verified via NuGet.org and GitHub).
- Architecture: HIGH — Cookie auth pattern is documented in Microsoft's official documentation with a dedicated article for the exact use case (cookie auth without Identity). The middleware pipeline order is enforced by the framework.
- Pitfalls: HIGH — Missing `UseAuthentication()` is a known ASP.NET Core 6+ issue discussed in GitHub issues and Stack Overflow. libphonenumber `Parse()` exception behavior is documented in the library source.

**Research date:** 2026-06-19
**Valid until:** 2026-08-19 (60 days — stable framework features, no known breaking changes in .NET 10 auth pipeline)
