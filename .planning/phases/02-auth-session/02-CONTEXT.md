# Phase 2: Auth (Session) - Context

**Gathered:** 2026-06-18
**Status:** Ready for planning

## Phase Boundary

This phase delivers session-based admin authentication and phone number validation/normalization. It locks down `/Admin/*` routes behind sign-in and sets up the identity primitives (phone normalization to E.164) that Phase 3's booking form will use.

**In scope:**
- Admin sign-in with username + password (session-based, no ASP.NET Core Identity)
- Admin sign-out (clear session, redirect to sign-in)
- First-run admin account provisioning (auto-detect + setup page)
- Phone number validation and normalization to E.164 format (`+959...`)
- Unauthenticated requests to `/Admin/*` redirect to sign-in page
- Nav bar shows Sign In / Sign Out conditionally based on auth state

**Out of scope (other phases):**
- User booking (Phase 3)
- Admin dashboard (Phase 4)
- Admin actions / notifications (Phase 5)
- Burmese formatting / mobile polish (Phase 6)

## Implementation Decisions

### Admin Account Provisioning
- **D-01:** First-run setup page at `/Auth/Setup` — accessible only when no admin exists in the database. Auto-detection via `!db.Users.Any(u => u.IsAdmin)`.
- **D-02:** Setup page is open (one-time access). No setup key or token. After the first admin is created, the route returns 404.
- **D-03:** Setup form collects: Username + Password (with confirmation field). Name and PhoneNumber are optional on setup — admin can add later.
- **D-04:** Password requirements: minimum 6 characters. No complexity rules (uppercase/number/special). The admin is the shop owner — don't add friction.
- **D-05:** After successful setup: show success message "Admin account created. Please sign in." → redirect to `/Auth/SignIn`.
- **D-06:** Setup page after admin exists → return 404 Not Found. Don't acknowledge the route ever existed.

### Admin Credential Model
- **D-07:** Admin signs in with Username + Password (not PIN, not phone number as credential).
- **D-08:** Add `Username` (string, required for admin) and `PasswordHash` (string) to the existing `User` entity. Regular users (non-admin) don't need Username — they identify by phone number only.
- **D-09:** Password hashing uses ASP.NET Core's built-in `PasswordHasher<T>` (or `BCrypt.Net-Next`). Store the hash, never the plaintext.

### Phone Number Validation
- **D-10:** Use `libphonenumber-csharp` for phone validation and normalization (per CLAUDE.md tech stack recommendation).
- **D-11:** Normalize to E.164 format (`+959...`). Accept Myanmar input formats: `09-xxx-xxxx`, `+959xxxxxxxx`, `959xxxxxxxx`.
- **D-12:** Invalid phone numbers are rejected server-side with a clear, Burmese-friendly error message.

### Claude's Discretion
Areas where the planner has flexibility to choose reasonable defaults:

- **Session timeout:** Suggest 2 hours with sliding expiration — long enough for an admin's workday, short enough to not leave sessions open indefinitely. Idle timeout resets on each request.
- **"Remember me":** Skip for v1. Single admin on their own device — not worth the complexity.
- **Sign-in UI:** Centered Bootstrap card on mobile, consistent with Home page card style (`shadow-sm`, borderless). Form fields: Username, Password, Sign In button (brand blue `#1877F2`).
- **Nav bar:** When signed out: "Sign In" link. When signed in: "Admin" link + "Sign Out" link. Display admin name in nav if available.
- **Auth failure message:** Generic "Invalid username or password" — don't reveal which field was wrong.
- **Session cookie:** `HttpOnly`, `Secure` (in production via HTTPS), `SameSite=Lax`.

## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Requirements
- `.planning/REQUIREMENTS.md` §Admin — ADMIN-01 (admin sign-in)
- `.planning/REQUIREMENTS.md` §Cross-Cutting — CROS-04 (phone validation/normalization)

### Architecture & Stack
- `CLAUDE.md` — "What NOT to Use": no ASP.NET Core Identity; session-based auth only; libphonenumber-csharp for phone validation
- `CLAUDE.md` — "Recommended Stack": ASP.NET Core MVC, EF Core, Npgsql, Bootstrap 5
- `.planning/PROJECT.md` — phone-only identity, no email/password in v1, in-app notifications

### Phase Context
- `.planning/ROADMAP.md` Phase 2 — success criteria, dependencies, requirements mapping
- `.planning/STATE.md` — Phase 01 complete, Phase 02 is the next phase

### Existing Code
- `ServiceCity.Core/Entities/User.cs` — User entity (Id, Name, PhoneNumber, PhoneNumberNormalized, IsAdmin, CreatedAt)
- `ServiceCity.Data/Configurations/UserConfiguration.cs` — Fluent API config (table, indexes, max lengths)
- `ServiceCity/Program.cs` — current middleware pipeline (needs AddAuthentication/AddSession/AddAuthorization)
- `ServiceCity/Controllers/HomeController.cs` — primary constructor DI pattern to follow
- `ServiceCity/Views/Shared/_Layout.cshtml` — nav bar to update with auth links
- `ServiceCity/Views/_ViewImports.cshtml` — tag helpers and namespaces available

## Existing Code Insights

### Reusable Assets
- **Primary constructor DI pattern** (HomeController): `public class AuthController(AppDbContext db) : Controller` — follow this for AuthController
- **Auto-migration pattern** (Program.cs): `IServiceScopeFactory` + `Database.Migrate()` — auto-migration already runs on startup, will pick up User entity changes
- **Card component style** (Home/Index.cshtml): Bootstrap 5 `shadow-sm` cards, brand blue accent — reuse for sign-in card layout
- **Entity Configuration pattern** (UserConfiguration.cs): Fluent API in Data project — add Username/PasswordHash config here

### Established Patterns
- Controller → EF Core query → View (no service layer yet) — use for AuthController in v1
- `async Task<IActionResult>` for DB-querying actions
- Partial views not used yet — stick with full Razor views

### Integration Points
- **`app.UseAuthorization()`** in Program.cs — middleware slot exists, needs auth services registered BEFORE it
- **Layout nav** (`_Layout.cshtml`): conditional nav items based on `User.Identity.IsAuthenticated`
- **`_ViewImports.cshtml`**: already imports `Core.Entities` and `Core.Enums` — User type available in views
- **Future Phase 3** will use phone normalization from this phase for booking submissions

## Deferred Ideas

None — discussion stayed within phase scope.

---

*Phase: 2-Auth (Session)*
*Context gathered: 2026-06-18*
