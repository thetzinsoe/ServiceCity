---
phase: 02-auth-session
plan: 01
subsystem: auth
tags: [aspnet-core, cookie-auth, pbkdf2, libphonenumber, ef-core, postgresql, razor-views, bootstrap-5]

# Dependency graph
requires:
  - phase: 01-project-scaffold-database
    provides: User entity (Id, Name, PhoneNumber, IsAdmin, CreatedAt), AppDbContext, auto-migration pattern, Bootstrap 5 _Layout.cshtml, Primary constructor DI pattern
provides:
  - User entity extended with Username and PasswordHash columns
  - EF Core AddAuthFields migration (adds Username nvarchar(200), PasswordHash text to Users table)
  - Cookie authentication services in Program.cs (2hr sliding, HttpOnly, SameSite=Lax)
  - AuthController with first-run Setup GET/POST actions
  - Phone number validation and E.164 normalization via libphonenumber-csharp
  - SetupViewModel with Username, Password, ConfirmPassword, Name, PhoneNumber
  - Setup.cshtml Razor view per UI-SPEC centered card layout
affects: [02-auth-session-plan-02, 03-user-booking, 04-admin-dashboard]

# Tech tracking
tech-stack:
  added:
    - libphonenumber-csharp v8.13.54 (phone validation and E.164 normalization)
  patterns:
    - CookieAuthenticationDefaults.AuthenticationScheme with AddCookie() for session-based admin auth
    - Primary constructor DI pattern (AuthController(AppDbContext db) : Controller)
    - PasswordHasher<T> for PBKDF2 password hashing (built-in, no Identity)
    - PhoneNumberUtil.GetInstance().Parse() + IsValidNumber() + Format(E164) with try-catch for NumberParseException
    - EF Core auto-migration pattern (db.Database.Migrate()) picks up new User columns on startup
    - Centered Bootstrap card layout (row justify-content-center + col-md-6 col-lg-4) for auth pages
    - asp-validation-summary="ModelOnly" + per-field span validation for server-side errors
    - TempData for redirect-to-get success messages

key-files:
  created:
    - ServiceCity/Controllers/AuthController.cs
    - ServiceCity/Models/SetupViewModel.cs
    - ServiceCity/Views/Auth/Setup.cshtml
    - ServiceCity.Data/Migrations/20260619180142_AddAuthFields.cs
    - ServiceCity.Data/Migrations/20260619180142_AddAuthFields.Designer.cs
  modified:
    - ServiceCity.Core/Entities/User.cs
    - ServiceCity.Data/Configurations/UserConfiguration.cs
    - ServiceCity.Data/Migrations/AppDbContextModelSnapshot.cs
    - ServiceCity/Program.cs
    - ServiceCity/ServiceCity.csproj

key-decisions:
  - "Used ASP.NET Core built-in PasswordHasher<T> (PBKDF2, HMAC-SHA256, 100k iterations) instead of BCrypt — ships in the shared framework, no NuGet dependency"
  - "Used libphonenumber-csharp v8.13.54 for phone validation per CLAUDE.md recommendation — handles Myanmar's 17+ mobile prefixes and E.164 normalization"
  - "Used CookieSecurePolicy.SameAsRequest instead of Always — enables HTTP dev without breaking cookie issuance"
  - "Phone validation is inline in AuthController for Phase 2; extraction to shared service deferred to Phase 3 when booking needs it"
  - "Admin Name defaults to Username when left blank on setup form — ensures every admin user has a display name"

patterns-established:
  - "Cookie auth without Identity: AddAuthentication(CookieDefaults).AddCookie() with UseAuthentication() before UseAuthorization()"
  - "First-run setup detection: GET and POST both check !db.Users.AnyAsync(u => u.IsAdmin) before allowing access"
  - "Phone validation wrapper: private static method returns (bool IsValid, string? Normalized, string? Error) tuple with try-catch for NumberParseException"
  - "Redirect-with-message: TempData[\"SuccessMessage\"] set before RedirectToAction for cross-request success display"

requirements-completed: [CROS-04]

# Metrics
duration: 17min
completed: 2026-06-19
status: complete
---

# Phase 02 Plan 01: Admin Setup Flow Summary

**Admin setup flow with ASP.NET Core cookie auth, PBKDF2 password hashing, and libphonenumber E.164 normalization for Myanmar phone numbers**

## Performance

- **Duration:** 17 min
- **Started:** 2026-06-19T17:52:46Z
- **Completed:** 2026-06-19T18:09:37Z
- **Tasks:** 3
- **Files modified:** 10

## Accomplishments
- User entity extended with Username (nvarchar 200) and PasswordHash (text) columns via EF Core migration
- Cookie authentication services registered with 2-hour sliding expiration, HttpOnly cookies, SameSite=Lax
- AuthController with first-run Setup GET (admin existence check) and POST (validation, PBKDF2 hashing, user creation, redirect)
- libphonenumber-csharp v8.13.54 added for Myanmar phone validation and E.164 normalization
- Setup.cshtml Razor view rendering per UI-SPEC: centered Bootstrap card, 5 form fields, brand-blue submit button, mobile tap targets

## Task Commits

Each task was committed atomically:

1. **Task 1: [BLOCKING] Add Username and PasswordHash to User entity, update UserConfiguration, generate and apply EF Core migration** - `21d3f01` (feat)
2. **Task 2: Register cookie auth services, add libphonenumber-csharp, create AuthController with Setup GET/POST and phone validation** - `f1ec799` (feat)
3. **Task 3: Create Setup.cshtml Razor view per UI-SPEC centered card layout** - `524457d` (feat)

## Files Created/Modified
- `ServiceCity.Core/Entities/User.cs` — Added `Username` and `PasswordHash` nullable string properties
- `ServiceCity.Data/Configurations/UserConfiguration.cs` — Fluent API config for Username (HasMaxLength 200) and PasswordHash
- `ServiceCity.Data/Migrations/20260619180142_AddAuthFields.cs` — EF Core migration adding Username and PasswordHash columns
- `ServiceCity.Data/Migrations/20260619180142_AddAuthFields.Designer.cs` — Migration designer file
- `ServiceCity.Data/Migrations/AppDbContextModelSnapshot.cs` — Updated model snapshot reflecting new columns
- `ServiceCity/Program.cs` — Added AddAuthentication(AddCookie), AddAuthorization, UseAuthentication before UseAuthorization
- `ServiceCity/ServiceCity.csproj` — Added libphonenumber-csharp v8.13.54 PackageReference
- `ServiceCity/Controllers/AuthController.cs` — Created with Setup GET/POST and ValidateAndNormalizePhone helper
- `ServiceCity/Models/SetupViewModel.cs` — Created with Username, Password, ConfirmPassword, Name, PhoneNumber
- `ServiceCity/Views/Auth/Setup.cshtml` — Created per UI-SPEC: centered card, 5 fields, antiforgery, validation summaries

## Decisions Made
- Used PasswordHasher<T> (built-in PBKDF2) instead of BCrypt.Net-Next — ships in ASP.NET Core shared framework, no extra dependency required per RESEARCH.md
- Used CookieSecurePolicy.SameAsRequest for dev compatibility — Always breaks cookie issuance on HTTP
- Admin Name defaults to Username when left blank — ensures every admin user has a display name in the nav bar
- Phone validation inline in AuthController — extraction to shared service deferred to Phase 3 per RESEARCH.md open question resolution

## Deviations from Plan

### Environment-specific handling

**Database already migrated from prior run**
- **Found during:** Task 1 (Migration generation and application)
- **Issue:** The `__EFMigrationsHistory` table contained `20260619174128_AddAuthFields` from a previous execution, meaning Username and PasswordHash columns already existed in the database. The migration code files for that entry did not exist on disk, so regenerating the migration produced ADD COLUMN statements that would fail on apply.
- **Resolution:** Regenerated the migration with a new timestamp (`20260619180142_AddAuthFields`), then manually inserted the migration history record since the DDL was already applied. This kept EF Core model snapshot and database schema in sync without data loss.
- **Files modified:** `ServiceCity.Data/Migrations/AppDbContextModelSnapshot.cs` (reset and regenerated)
- **Committed in:** `21d3f01`

**Docker SDK used for all dotnet operations**
- **Issue:** .NET SDK 10.0 not installed on the host machine. All `dotnet build`, `dotnet ef migrations add`, and `dotnet ef database update` commands were executed via `docker run mcr.microsoft.com/dotnet/sdk:10.0` with volume mounts. PostgreSQL was already running via the existing `servicecity-db` container.
- **Impact:** None — all operations completed successfully inside Docker containers with identical results to host-native execution.

## Issues Encountered
- Root-owned files from Docker containers required `chown` fix before git staging (resolved via `docker run --user root chown -R 1000:1000`)
- NETSDK1064 analyzer package cache issue when building across separate Docker runs (resolved by running restore + build in single container session)

## User Setup Required

None — no external service configuration required. The auto-migration pattern in Program.cs (`db.Database.Migrate()`) applies the AddAuthFields migration on app startup. When running `docker compose up`, the migration will execute automatically.

## Next Phase Readiness
- Admin identity model complete — Username and PasswordHash columns exist on User entity
- Cookie auth middleware pipeline registered and functional
- First-run setup page is ready at `/Auth/Setup` — provisions the initial admin when no admin exists
- Phone validation helper (ValidateAndNormalizePhone) available for Phase 3 booking form reuse
- Plan 02 (SignIn/SignOut flow) can build on the auth services and entity changes from this plan

---
*Phase: 02-auth-session*
*Completed: 2026-06-19*
