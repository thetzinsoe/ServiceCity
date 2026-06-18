# Phase 2: Auth (Session) - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-06-18
**Phase:** 02-auth-session
**Areas discussed:** Admin account provisioning

---

## Admin Account Provisioning

### Provisioning method — how does the initial admin get created?

| Option | Description | Selected |
|--------|-------------|----------|
| Auto-detect + setup page | Check if any admin exists on startup. If not, `/Auth/Setup` is accessible. First person creates admin. After creation, route returns 404. | ✓ |
| Environment variables | Admin credentials from `docker-compose.yml` or `.env`. App reads on startup and creates admin if missing. | |
| Hybrid: env vars with setup fallback | Check env vars first; auto-create admin. If not set, fall back to setup page. | |

**User's choice:** Auto-detect + setup page (Recommended)
**Notes:** User initially said "I think option 'b' can be okay?" (referring to setup page approach). Clarified that auto-detect + setup page is simplest — no credentials in git, zero config for Docker deploy, one-time page like WordPress.

### Setup page gating — how to prevent random visitors from claiming admin?

| Option | Description | Selected |
|--------|-------------|----------|
| Open (one-time) | Page accessible when no admin exists. Dead after first admin created (404). Same pattern as WordPress, Ghost. | ✓ |
| Setup key/token | Token printed to Docker logs or in `.env`. Setup page requires token. | |

**User's choice:** Open (one-time)

### Setup form fields — what does the admin fill in?

| Option | Description | Selected |
|--------|-------------|----------|
| Username + Password | Simple two-field form. Phone and name added later or optional. | ✓ |
| Phone + PIN | Phone number as identifier + 4-6 digit PIN. Very mobile-friendly but weaker security. | |
| Username + Password + Phone + Name | Full form with optional fields. Most complete but most fields for first-run. | |

**User's choice:** Username + Password (Recommended)

### Identity storage — where to store admin credentials?

| Option | Description | Selected |
|--------|-------------|----------|
| Add Username to User entity | Add `Username` and `PasswordHash` to existing User entity. One table, simple queries. | ✓ |
| Separate AdminCredential table | New table with Username + PasswordHash + UserId FK. More normalized but more complex. | |

**User's choice:** Add Username to User entity (Recommended)

### Password requirements — what rules on the setup form?

| Option | Description | Selected |
|--------|-------------|----------|
| Minimum length only | 6-8 chars minimum. No complexity rules. Don't frustrate the shop owner. | ✓ |
| Standard complexity | 8 chars, uppercase, lowercase, digit. More secure but adds friction. | |
| No requirements | Accept anything. Simplest setup. Risk of very weak production passwords. | |

**User's choice:** Minimum length only (Recommended)

### Post-setup flow — what happens after successful admin creation?

| Option | Description | Selected |
|--------|-------------|----------|
| Redirect to sign-in | Success message → redirect to `/Auth/SignIn`. Clean separation: setup is one-time, sign-in is normal. | ✓ |
| Auto sign-in | Create admin and immediately sign in → redirect to `/Admin/Dashboard`. Smoother but admin never tests sign-in flow. | |

**User's choice:** Redirect to sign-in (Recommended)

### Setup page after admin exists — what if someone visits the URL later?

| Option | Description | Selected |
|--------|-------------|----------|
| 404 Not Found | Standard 404. The page doesn't exist anymore — don't acknowledge it was ever there. | ✓ |
| Redirect to sign-in | Redirect to `/Auth/SignIn`. Better UX for revisiting admin but confirms the page existed. | |

**User's choice:** 404 Not Found (Recommended)

---

## Claude's Discretion

The following areas were identified but not discussed — the planner will use reasonable defaults:

- **Session timeout:** 2 hours with sliding expiration
- **"Remember me":** Skip for v1
- **Sign-in UI layout:** Centered Bootstrap card, brand blue (#1877F2), consistent with Home page card style
- **Nav bar:** Conditional Sign In / Sign Out + Admin links based on auth state
- **Auth failure message:** Generic "Invalid username or password"
- **Session cookie:** HttpOnly, Secure (in production), SameSite=Lax

## Deferred Ideas

None — discussion stayed within phase scope.
