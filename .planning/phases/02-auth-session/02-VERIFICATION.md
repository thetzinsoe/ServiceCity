---
phase: 02-auth-session
verified: 2026-06-19T18:30:00Z
status: human_needed
score: 12/16 must-haves verified
behavior_unverified: 4
overrides_applied: 0
behavior_unverified_items:
  - truth: "Submitting valid setup form creates admin user with hashed password and redirects to /Auth/SignIn with success message"
    test: "Navigate to /Auth/Setup, fill form with valid data, submit. Observe redirect to /Auth/SignIn and TempData success message display."
    expected: "Admin user row created in database with hashed PasswordHash, browser redirected to /Auth/SignIn, success message displayed."
    why_human: "Redirect behavior and TempData cross-request success message require server runtime — grep can confirm RedirectToAction and TempData assignment exist, but cannot verify the redirect chain or message rendering."
  - truth: "Admin navigates to /Auth/SignIn, enters correct Username and Password, and is redirected to /Admin/Dashboard"
    test: "Navigate to /Auth/SignIn, enter correct admin credentials, submit. Observe redirect to /Admin/Dashboard and cookie issuance."
    expected: "Browser redirected to /Admin/Dashboard, auth cookie set (HttpOnly, SameSite=Lax), ClaimsPrincipal populated with NameIdentifier, Name, Role claims."
    why_human: "Full sign-in flow requires server runtime — cookie issuance, authentication middleware, ClaimsPrincipal construction, and redirect are end-to-end behaviors grep cannot exercise."
  - truth: "Admin clicks Sign Out from any page and is redirected to /Auth/SignIn"
    test: "While signed in, click 'Sign Out' in the navigation bar. Observe redirect to /Auth/SignIn and cookie cleared."
    expected: "Auth cookie cleared, browser redirected to /Auth/SignIn, navigation bar switches from 'Admin | Sign Out' to 'Sign In'."
    why_human: "Cookie clearance and navigation bar state toggling require server runtime and actual authentication state — code presence confirms the SignOutAsync + RedirectToAction call, but the full sign-out experience needs a browser."
  - truth: "Unauthenticated requests to /Admin or /Admin/* redirect to /Auth/SignIn"
    test: "While signed out, navigate to /Admin/Dashboard. Observe redirect to /Auth/SignIn (with or without ReturnUrl query parameter)."
    expected: "Browser redirected to /Auth/SignIn. The [Authorize] attribute + LoginPath cookie auth option must work together. Order of UseAuthentication() before UseAuthorization() is critical."
    why_human: "Middleware pipeline behavior is a runtime concern — code confirms [Authorize] attribute, LoginPath setting, and correct middleware ordering, but the actual redirect requires executing the pipeline with an unauthenticated request."
human_verification:
  - test: "Full admin sign-up and sign-in flow: docker compose up -d, navigate to /Auth/Setup, create admin account, sign in at /Auth/SignIn"
    expected: "Setup form creates admin row with hashed password. Sign-in issues auth cookie. Redirected to /Admin/Dashboard showing 'Admin Dashboard' heading. Nav shows Admin + Sign Out."
    why_human: "Complete end-to-end auth flow requires running Docker containers with PostgreSQL and the ASP.NET Core app. Redirect chains, cookie issuance, and TempData message display are runtime behaviors."
  - test: "Route protection: navigate to /Admin/Dashboard while signed out"
    expected: "Redirected to /Auth/SignIn. The LoginPath cookie auth option and [Authorize] attribute on AdminController must work together."
    why_human: "Middleware pipeline routing and cookie auth challenge require server runtime."
  - test: "Phone number validation: enter 'abc' as phone number on setup form"
    expected: "Form redisplays with error 'Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789).' The error must appear on the page, not as a raw exception."
    why_human: "Error display on form requires server-side rendering of ModelState errors — code confirms the catch block and ModelState.AddModelError call, but the rendered HTML output needs visual verification."
  - test: "Invalid credentials: enter wrong password at /Auth/SignIn"
    expected: "Page reloads with 'Invalid username or password.' in the validation summary. No indication of which field (username or password) was wrong. Identical message for wrong username case."
    why_human: "Generic error message rendering and the ModelOnly validation summary behavior require runtime — the message is identical in code for both failure cases, but the rendered output and the absence of field-level hints need visual confirmation."
  - test: "Navigation bar state: verify nav links change based on auth state"
    expected: "Signed out: nav shows only 'Sign In' link. Signed in: nav shows 'Admin' and 'Sign Out' links. Sign Out is a POST form with button, not a GET anchor tag."
    why_human: "Conditional rendering based on User.Identity.IsAuthenticated requires actual auth cookie presence — code confirms the Razor if/else blocks, but the visual switching needs browser verification."
  - test: "Phone number normalization: enter '09-123-456-789' and '+959123456789' on separate setup attempts"
    expected: "Both normalize to '+959123456789' in the PhoneNumberNormalized column. libphonenumber-csharp E.164 formatting is deterministic."
    why_human: "Actual libphonenumber library behavior at runtime — the code path is complete, but country-specific parsing and formatting need runtime verification with the installed library version."
  - test: "MVP mode format issue: Phase goal is not in User Story format"
    expected: "Phase goal should be 'As a shop admin, I want to sign in with credentials and have phone numbers validated for all users, so that I can securely access the admin dashboard and user data is consistent.' — run `/gsd mvp-phase 2` to set a proper User Story goal, or accept the override below."
    why_human: "User Story format is required for MVP mode phases to enable structured User Flow Coverage verification. The current goal 'Admin can sign in and out. Phone numbers are validated and normalized for all user identity operations.' does not match the 'As a ..., I want to ..., so that ...' pattern."
gaps: []
deferred:
  - truth: "/Admin/Dashboard shows only placeholder heading — full dashboard deferred to Phase 4"
    addressed_in: "Phase 4"
    evidence: "Phase 4 goal: 'Admin sees all bookings grouped by status on a dashboard and can click into any booking for full details.' Success criteria include booking grouping and detail view. The placeholder is intentional per Phase 2 plan."
---

# Phase 02: Auth (Session) Verification Report

**Phase Goal:** Admin can sign in and out. Phone numbers are validated and normalized for all user identity operations.
**Mode:** MVP
**Verified:** 2026-06-19T18:30:00Z
**Status:** human_needed
**Re-verification:** No — initial verification

> **WARNING — MVP Mode Format Issue:** The phase goal is not in User Story format (`As a ..., I want to ..., so that ...`). The MVP mode User Story format guard returned `valid: false`. Run `/gsd mvp-phase 2` to set a proper User Story goal, or accept this non-standard format as an override. This verification proceeds using standard goal-backward methodology against the roadmap success criteria, without the MVP-specific User Flow Coverage table.

## Goal Achievement

### Observable Truths — Roadmap Success Criteria

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Admin navigates to `/Auth/SignIn`, enters correct credentials, and is redirected to the admin dashboard (placeholder) | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | SignIn GET returns View (line 76-79). SignIn POST queries User by Username, verifies hash with `PasswordHasher.VerifyHashedPassword`, builds ClaimsPrincipal (NameIdentifier, Name, Role claims), calls `HttpContext.SignInAsync`, returns `RedirectToAction("Dashboard", "Admin")` (lines 82-114). Admin/Dashboard.cshtml exists with `<h1>Admin Dashboard</h1>`. All code is present and wired — redirect chain and cookie issuance require server runtime. |
| 2 | Admin can sign out from any page and is redirected to the sign-in page | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | SignOut POST calls `HttpContext.SignOutAsync` then `RedirectToAction("SignIn")` (lines 117-121). Navigation bar renders Sign Out as `<form method="post">` with `<button type="submit">` styled as nav-link (Layout line 37-39) — POST-only per CSRF mitigation. All code is present — cookie clearance and redirect require server runtime. |
| 3 | Unauthenticated requests to `/Admin/*` redirect to the sign-in page | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | AdminController has class-level `[Authorize]` attribute (AdminController.cs line 6). Cookie auth configured with `LoginPath = "/Auth/SignIn"` (Program.cs line 14). Middleware order is correct: `app.UseAuthentication()` before `app.UseAuthorization()` (Program.cs lines 46-47). Wiring is complete — actual middleware redirect requires runtime. |
| 4 | Phone number input accepts Myanmar formats (09-xxx, +959xxx) and stores normalized E.164 format | ✓ VERIFIED | `ValidateAndNormalizePhone` method (AuthController.cs lines 123-140) uses `PhoneNumberUtil.GetInstance().Parse(input, "MM")`, checks `IsValidNumber()`, formats with `PhoneNumberFormat.E164` producing `+959...` format. Normalized value stored in `user.PhoneNumberNormalized`. libphonenumber-csharp v8.13.54 installed in csproj (line 11). Setup form uses `type="tel"` with placeholder "e.g., 09-xxx-xxxx". |
| 5 | Invalid phone numbers (too short, letters, wrong country code) are rejected with a clear error message | ✓ VERIFIED | `NumberParseException` caught with message: "Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789)." (line 137-138). Invalid-but-parseable numbers return: "Please enter a valid Myanmar phone number." (line 131). Both errors added via `ModelState.AddModelError("PhoneNumber", ...)`. Error surfaces in view via `<span asp-validation-for="PhoneNumber">`. |

**Score (roadmap):** 2/5 verified, 3 present but behavior-unverified

### Observable Truths — Plan Must-Haves (Deduplicated)

| # | Source | Truth | Status | Evidence |
|---|--------|-------|--------|----------|
| P01-1 | 02-01 | When no admin exists in database, `/Auth/Setup` renders the setup form | ✓ VERIFIED | `[HttpGet] Setup()` checks `await db.Users.AnyAsync(u => u.IsAdmin)`, returns `NotFound()` if true, `View()` otherwise (lines 17-22). |
| P01-2 | 02-01 | Setup form accepts Username, Password, ConfirmPassword (required) and Name, PhoneNumber (optional) | ✓ VERIFIED | SetupViewModel has `[Required]` on Username, Password, ConfirmPassword. Name and PhoneNumber are `string?` (nullable). |
| P01-3 | 02-01 | Submitting valid setup form creates admin user with hashed password and redirects to `/Auth/SignIn` with success message | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | Setup POST validates, hashes password via `PasswordHasher<User>().HashPassword(null!, model.Password)`, creates User entity with `IsAdmin=true`, sets `TempData["SuccessMessage"] = "Admin account created. Please sign in."`, returns `RedirectToAction("SignIn")` (lines 24-72). |
| P01-4 | 02-01 | When admin exists in database, `/Auth/Setup` returns 404 Not Found | ✓ VERIFIED | Both GET (line 20) and POST (line 28) check `await db.Users.AnyAsync(u => u.IsAdmin)` and return `NotFound()` if true. |
| P01-5 | 02-01 | Phone numbers entered in Myanmar formats (09-xxx, +959xxx) are validated and normalized to E.164 (+959...) | ✓ VERIFIED | Same as SC 4 above. `PhoneNumberUtil.Format(number, PhoneNumberFormat.E164)` produces E.164. |
| P01-6 | 02-01 | Invalid phone numbers are rejected with a clear error message | ✓ VERIFIED | Same as SC 5 above. Try-catch for `NumberParseException`, user-friendly messages. |
| P01-7 | 02-01 | Password must be at least 6 characters; mismatch is rejected | ✓ VERIFIED | `if (model.Password.Length < 6)` adds error "Password must be at least 6 characters." (lines 32-36). `if (model.Password != model.ConfirmPassword)` adds error "Passwords do not match." (lines 38-42). |
| P01-8 | 02-01 | Admin can complete setup when Name and PhoneNumber fields are left blank | ✓ VERIFIED | `Name = model.Name ?? model.Username` (line 61) — defaults Name to Username. `PhoneNumber = model.PhoneNumber ?? ""` (line 62) — defaults to empty. |
| P01-9 | 02-01 | Existing database rows (service categories, bookings) survive the migration unchanged | ✓ VERIFIED | Migration adds only `PasswordHash text NULL` and `Username character varying(200) NULL` columns — no data deletion or column removal. Down method only `DropColumn` if rolled back. |
| P02-1 | 02-02 | Admin navigates to `/Auth/SignIn`, enters correct Username and Password, and is redirected to `/Admin/Dashboard` | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | Same as SC 1 above. SignInAsync + RedirectToAction wiring is complete. |
| P02-2 | 02-02 | Admin enters incorrect credentials, sees "Invalid username or password." without revealing which field was wrong | ✓ VERIFIED | Both "user not found" (line 89) and "wrong password" (line 97) use identical `ModelState.AddModelError(string.Empty, "Invalid username or password.")`. Error key is `string.Empty` — captured by `asp-validation-summary="ModelOnly"`, not field-specific. Uses `PasswordVerificationResult.Failed` (not `!= Success`) to allow `SuccessRehashNeeded` to pass. |
| P02-3 | 02-02 | Admin clicks Sign Out from any page and is redirected to `/Auth/SignIn` | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | Same as SC 2 above. `SignOutAsync` + `RedirectToAction("SignIn")` wiring complete. |
| P02-4 | 02-02 | Unauthenticated requests to `/Admin` or `/Admin/*` redirect to `/Auth/SignIn` | ⚠️ PRESENT_BEHAVIOR_UNVERIFIED | Same as SC 3 above. `[Authorize]` + `LoginPath` + middleware order all correct. |
| P02-5 | 02-02 | Navigation bar shows "Sign In" link when signed out | ✓ VERIFIED | _Layout.cshtml else branch: `<a class="nav-link text-dark" asp-controller="Auth" asp-action="SignIn">Sign In</a>` (lines 44-46). |
| P02-6 | 02-02 | Navigation bar shows "Admin" and "Sign Out" links when signed in | ✓ VERIFIED | _Layout.cshtml if branch: "Admin" link (line 34) and POST-only "Sign Out" form with button styled as nav-link (lines 37-39). Uses `User.Identity?.IsAuthenticated == true` for conditional rendering. |
| P02-7 | 02-02 | `/Admin/Dashboard` displays "Admin Dashboard" heading inside the standard layout | ✓ VERIFIED | Dashboard.cshtml has `<h1>Admin Dashboard</h1>` (line 5). Uses default _Layout.cshtml via `_ViewStart.cshtml`. |

**Score:** 12/16 truths verified (12 code-complete, 4 present but behavior-unverified)

### Deferred Items

Items intentionally incomplete — addressed in later milestone phases.

| # | Item | Addressed In | Evidence |
|---|------|-------------|----------|
| D-1 | Admin Dashboard is a placeholder (`<h1>Admin Dashboard</h1>`) — no booking data, status grouping, or detail views | Phase 4 | Phase 4 goal: "Admin sees all bookings grouped by status on a dashboard and can click into any booking for full details." Success criteria cover status-grouped sections, booking detail page, mobile-responsive card layout. The placeholder is intentional — enables [Authorize] route protection and nav link wiring without building the full dashboard. |

### Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `ServiceCity.Core/Entities/User.cs` | Contains `Username` and `PasswordHash` nullable string properties | ✓ VERIFIED | Lines 9-10: `public string? Username { get; set; }`, `public string? PasswordHash { get; set; }`. Both nullable. |
| `ServiceCity.Data/Configurations/UserConfiguration.cs` | Fluent API config for Username (HasMaxLength 200) and PasswordHash | ✓ VERIFIED | Lines 26-27: `.Property(u => u.Username).HasMaxLength(200)`, line 29: `.Property(u => u.PasswordHash)`. |
| `ServiceCity/Controllers/AuthController.cs` | Setup GET/POST, SignIn GET/POST, SignOut POST, ValidateAndNormalizePhone | ✓ VERIFIED | 141 lines. All 5 public actions + 1 private helper present. Primary constructor DI with `AppDbContext db`. |
| `ServiceCity/Models/SetupViewModel.cs` | 5 properties: Username, Password, ConfirmPassword, Name, PhoneNumber | ✓ VERIFIED | 19 lines. 3 `[Required]` properties, 2 optional (`string?`). |
| `ServiceCity/Models/SignInViewModel.cs` | 2 properties: Username, Password with DataType | ✓ VERIFIED | 13 lines. `[Required]` on both, `[DataType(DataType.Password)]` on Password. |
| `ServiceCity/Views/Auth/Setup.cshtml` | Centered card layout per UI-SPEC, 5 fields | ✓ VERIFIED | 56 lines. `@model SetupViewModel`, centered card (`row justify-content-center` + `col-md-6 col-lg-4` + `card shadow-sm`), 5 form fields in correct order, `@Html.AntiForgeryToken()`, `asp-validation-summary="ModelOnly"`, `btn-primary w-100` with `min-height: 48px`, "Create Account" button text, Scripts section. |
| `ServiceCity/Views/Auth/SignIn.cshtml` | Centered card layout, 2 fields | ✓ VERIFIED | 38 lines. `@model SignInViewModel`, matching card pattern, 2 fields only (Username, Password), no Remember Me, `btn-primary w-100` with `min-height: 48px`, "Sign In" button text, ModelOnly validation summary, Scripts section. |
| `ServiceCity/Views/Admin/Dashboard.cshtml` | Placeholder dashboard | ✓ VERIFIED | 6 lines. `<h1>Admin Dashboard</h1>` inside standard layout. Intentional placeholder — Phase 4 replaces. |
| `ServiceCity/Controllers/AdminController.cs` | [Authorize]-protected, Dashboard action | ✓ VERIFIED | 13 lines. Class-level `[Authorize]` attribute, `Dashboard()` returns `View()`. No DB context needed. |
| `ServiceCity/Views/Shared/_Layout.cshtml` | Conditional nav links based on auth state | ✓ VERIFIED | Lines 30-48. Second `<ul class="navbar-nav ms-auto">` with `if (User.Identity?.IsAuthenticated == true)`. "Admin" + POST-only "Sign Out" form when signed in, "Sign In" link when signed out. |
| `ServiceCity/Program.cs` | Cookie auth registration + middleware | ✓ VERIFIED | Lines 11-22: `AddAuthentication(CookieDefaults).AddCookie()` with LoginPath, LogoutPath, AccessDeniedPath, 2hr ExpireTimeSpan, SlidingExpiration, HttpOnly, SameSite=Lax, SameAsRequest SecurePolicy. Lines 24, 46, 47: `AddAuthorization()`, `UseAuthentication()` before `UseAuthorization()`. |
| `ServiceCity/ServiceCity.csproj` | libphonenumber-csharp v8.13.54 | ✓ VERIFIED | Line 11: `<PackageReference Include="libphonenumber-csharp" Version="8.13.54" />`. |
| `ServiceCity.Data/Migrations/20260619180142_AddAuthFields.cs` | Migration adding Username + PasswordHash columns | ✓ VERIFIED | Up: `AddColumn<string>("PasswordHash", type: "text", nullable: true)`, `AddColumn<string>("Username", type: "character varying(200)", maxLength: 200, nullable: true)`. Down: `DropColumn` for both. No data loss. |

### Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `AuthController.cs` | `User.cs` entity | `db.Users.AnyAsync(u => u.IsAdmin)` and `db.Users.Add(user)` and `db.Users.FirstOrDefaultAsync(u => u.Username == ...)` | ✓ WIRED | 3 distinct DB operations on Users table across actions (lines 19, 27, 68, 86). |
| `AuthController.cs` | `AppDbContext.cs` | Primary constructor DI: `AuthController(AppDbContext db) : Controller` | ✓ WIRED | Line 14: `public class AuthController(AppDbContext db) : Controller`. Matches established DI pattern from HomeController. |
| `Program.cs` | `AuthController.cs` | Cookie auth `LoginPath="/Auth/SignIn"` directs unauthenticated requests | ✓ WIRED | Line 14: `options.LoginPath = "/Auth/SignIn"`. Line 16: `AccessDeniedPath = "/Auth/SignIn"`. |
| `AdminController.cs` | `Program.cs` | `[Authorize]` attribute triggers cookie auth middleware redirect | ✓ WIRED | Line 6: `[Authorize]` class-level attribute. Cookie auth middleware configured with LoginPath/AccessDeniedPath. |
| `_Layout.cshtml` | `AuthController.cs` | `asp-controller="Auth"` links for SignIn and SignOut | ✓ WIRED | Line 37: `asp-controller="Auth" asp-action="SignOut"`. Line 45: `asp-controller="Auth" asp-action="SignIn"`. |
| `AuthController.cs` | `Program.cs` | `HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ...)` | ✓ WIRED | Line 111: `await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal)`. Scheme matches Program.cs registration. |

### Data-Flow Trace (Level 4)

| Artifact | Data Variable | Source | Produces Real Data | Status |
|----------|--------------|--------|-------------------|--------|
| `AuthController.Setup GET` | `hasAdmin` | `db.Users.AnyAsync(u => u.IsAdmin)` — real EF Core query | Yes — actual DB query | ✓ FLOWING |
| `AuthController.Setup POST` | `user` entity | `new User { ... }` + `db.Users.Add(user)` + `SaveChangesAsync()` | Yes — writes real data to DB | ✓ FLOWING |
| `AuthController.SignIn POST` | `user` entity | `db.Users.FirstOrDefaultAsync(u => u.Username == model.Username)` — real EF Core query | Yes — actual DB query | ✓ FLOWING |
| `Admin/Dashboard.cshtml` | N/A — static placeholder | No dynamic data | N/A — intentional placeholder for Phase 4 | N/A (static) |
| `_Layout.cshtml` nav | `User.Identity?.IsAuthenticated` | Populated by cookie auth middleware after `UseAuthentication()` | Yes — runtime auth state | ✓ FLOWING |

### Behavioral Spot-Checks

No runnable entry points available — .NET SDK 10.0 is not on the host PATH (documented in SUMMARY.md). All builds are performed via Docker SDK image.

| Behavior | Command | Result | Status |
|----------|---------|--------|--------|
| dotnet build | Not runnable — SDK not on PATH | N/A | ? SKIP |
| Phone validation method | Not runnable without runtime | N/A | ? SKIP |
| Migration SQL | Verified via file read — migration adds Username + PasswordHash | Verified | ✓ PASS |

### Probe Execution

No probes declared in PLAN or SUMMARY files for this phase. Phase is auth implementation, not a migration/tooling phase.

| Probe | Result | Status |
|-------|--------|--------|
| N/A | No probes declared | ? SKIP |

### Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|------------|-------------|--------|----------|
| ADMIN-01 | 02-02-PLAN | Admin can sign in with credentials (username + password/PIN) to access the dashboard | ✓ SATISFIED | Complete sign-in/sign-out vertical slice implemented: Setup creates admin with hashed password (02-01), SignIn authenticates via PasswordHasher.VerifyHashedPassword + ClaimsPrincipal + SignInAsync (02-02), AdminController protected by [Authorize], placeholder dashboard at /Admin/Dashboard. Nav bar conditionally renders auth links. |
| CROS-04 | 02-01-PLAN | Phone numbers are validated and normalized to a consistent format | ✓ SATISFIED | `ValidateAndNormalizePhone` uses libphonenumber-csharp v8.13.54: `PhoneNumberUtil.Parse(input, "MM")` with try-catch for `NumberParseException`, `IsValidNumber()` check, `Format(E164)` normalization. Invalid numbers rejected with user-friendly error messages. Setup form uses `type="tel"` for mobile keyboard. |

### Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `Setup.cshtml` | 25 | `placeholder="Must be at least 6 characters."` | ℹ️ Info | Legitimate HTML form field placeholder — not a code-level TODO/placeholder. |
| `Setup.cshtml` | 42 | `placeholder="e.g., 09-xxx-xxxx"` | ℹ️ Info | Legitimate HTML form field placeholder — not a code-level TODO/placeholder. |

**No blockers found.** No `TBD`, `FIXME`, or `XXX` markers in any phase-modified file. No empty return implementations. No hardcoded empty data in rendering paths. The only `placeholder` matches are HTML form input placeholders, not code stubs.

### Human Verification Required

The following items need manual testing with a running server (Docker). These cannot be verified by file inspection alone because they involve HTTP redirects, cookie state, authentication middleware, and rendered HTML output.

#### 1. Full Admin Setup + Sign-In Flow

**Test:** Run `docker compose up -d`, navigate to `/Auth/Setup`, create admin account with username + password, then sign in at `/Auth/SignIn` with the same credentials.
**Expected:** Setup creates admin row with PBKDF2-hashed password. SignIn issues HttpOnly auth cookie. Redirected to `/Admin/Dashboard` showing "Admin Dashboard" heading. Nav bar shows "Admin" and "Sign Out" links.
**Why human:** Complete end-to-end flow with Docker, PostgreSQL, cookie issuance, redirect chains.

#### 2. Route Protection — Unauthenticated Access

**Test:** While signed out, navigate to `/Admin/Dashboard` directly in the browser.
**Expected:** Redirected to `/Auth/SignIn` (with or without `ReturnUrl` query parameter). The `[Authorize]` attribute on AdminController combined with `LoginPath="/Auth/SignIn"` cookie auth options must enforce the redirect.
**Why human:** ASP.NET Core middleware pipeline behavior — requires actual HTTP request processing.

#### 3. Phone Number Validation — Invalid Input

**Test:** On the setup form, enter `"abc"` as the phone number and submit.
**Expected:** Form redisplays with error message "Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789)." No raw exception text. The error appears in the validation summary or phone number field span.
**Why human:** Error rendering in Razor view requires server-side ModelState processing.

#### 4. Credential Error — Generic Message

**Test:** Sign in with wrong password (correct username) and then with wrong username.
**Expected:** Both cases show "Invalid username or password." in the ModelOnly validation summary. No field-level indication (Username or Password span should be empty). Identical message for both failure modes — prevents username enumeration.
**Why human:** Generic error rendering and absence of field-level hints need visual confirmation.

#### 5. Navigation Bar Auth State

**Test:** Observe the navigation bar while signed out and while signed in.
**Expected:** Signed out: only "Sign In" link visible. Signed in: "Admin" and "Sign Out" links visible. Sign Out is a `<form method="post">` with `<button type="submit">` (not an `<a>` GET link). The `ms-auto` class right-aligns the auth nav section.
**Why human:** Conditional Razor rendering based on `User.Identity.IsAuthenticated` requires actual auth cookie state.

#### 6. Phone Number Normalization

**Test:** Submit the setup form with phone number `"09-123-456-789"` and separately with `"+959123456789"`.
**Expected:** Both normalize to `"+959123456789"` stored in the `PhoneNumberNormalized` column. Verify via `docker compose exec db psql -U servicecity -d servicecity -c 'SELECT "PhoneNumberNormalized" FROM "Users";'`.
**Why human:** libphonenumber-csharp library behavior at runtime — the code path is complete but actual parsing/formatting output needs confirmation.

### Gaps Summary

**No implementation gaps found.** All 16 must-have truths have corresponding code in the codebase. All 14 required artifacts exist on disk, are substantive (not stubs), and are wired into the application. All 6 key links are connected. Requirements ADMIN-01 and CROS-04 are traceably satisfied.

**4 truths require human verification** for their redirect/auth-state behavior. These truths have complete code implementations — the sign-in flow, sign-out, route protection, and setup-to-sign-in redirect are all fully wired — but the actual runtime behavior (cookie issuance, middleware redirects, HTTP 302 chains) requires a running server to confirm.

**1 known stub** is intentional: `ServiceCity/Views/Admin/Dashboard.cshtml` contains only `<h1>Admin Dashboard</h1>` with no booking data or status grouping. This is deferred to Phase 4 (Admin Dashboard). Defers to Phase 4 per roadmap — not a gap.

**MVP Mode format note:** The phase goal is not in User Story format. This does not affect the verification results but means the MVP-specific User Flow Coverage table was not generated. Consider running `/gsd mvp-phase 2` to set a proper User Story goal.

---

_Verified: 2026-06-19T18:30:00Z_
_Verifier: Claude (gsd-verifier)_
