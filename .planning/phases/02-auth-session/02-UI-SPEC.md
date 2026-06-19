---
phase: 02
slug: auth-session
status: draft
shadcn_initialized: false
preset: none
created: 2026-06-19
---

# Phase 02 — UI Design Contract

> Visual and interaction contract for Phase 02: Auth (Session). Generated from upstream artifacts (REQUIREMENTS.md, CONTEXT.md, RESEARCH.md) with locked design decisions.

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none (Bootstrap 5.3.x CDN, server-rendered Razor views) |
| Preset | not applicable |
| Component library | Bootstrap 5.3.x (via CDN) |
| Icon library | none (v1) |
| Font | Bootstrap default system font stack |

---

## Spacing Scale

Bootstrap 5 defaults (multiples of 4). Explicit tokens for form layouts:

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | — |
| sm | 8px | — |
| md | 16px | Form field gaps (mb-3), card padding (card-body) |
| lg | 24px | Card margin from viewport edge on mobile (container padding) |
| xl | 32px | — |
| 2xl | 48px | — |
| 3xl | 64px | — |

Exceptions: none

---

## Typography

Bootstrap 5 default type scale. Overrides for auth pages:

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 1rem (16px) | 400 | 1.5 |
| Label | 1rem (16px) | 400 | 1.5 |
| Heading (page title) | 1.75rem (~28px, h3) | 600 | 1.3 |
| Error text | 0.875rem (14px) | 400 | 1.5 |

Page headings use Bootstrap's `.h3` class (font-size: 1.75rem, font-weight: 600).

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | #FFFFFF | Page background, card background |
| Secondary (30%) | #F0F2F5 | Card grouping on Home page (existing), contextual background |
| Accent (10%) | #1877F2 | Primary CTA buttons (Sign In, Create Account), links, active states, navbar brand |
| Destructive | #DC3545 | Bootstrap `.btn-danger`, used for Sign Out button (if styled explicitly) |

Accent reserved for: primary submit buttons, text links (`.btn-link`), navbar brand color.

---

## Copywriting Contract

| Element | Copy |
|---------|------|
| Setup page heading | Set Up Admin Account |
| Setup page description | Create the administrator account for ServiceCity. This page is available only once. |
| Setup form — Username label | Username |
| Setup form — Password label | Password |
| Setup form — Confirm Password label | Confirm Password |
| Setup form — Name label | Name (optional) |
| Setup form — Phone Number label | Phone Number (optional) |
| Setup CTA | Create Account |
| Setup success message | Admin account created. Please sign in. |
| SignIn page heading | Sign In |
| SignIn form — Username label | Username |
| SignIn form — Password label | Password |
| SignIn CTA | Sign In |
| Auth failure message | Invalid username or password. |
| Nav — signed out | Sign In |
| Nav — signed in (Admin link) | Admin |
| Nav — signed in (Sign Out link) | Sign Out |
| Admin Dashboard (placeholder) | Admin Dashboard |
| Empty state heading | n/a (Phase 02 has no empty states — Setup page IS the initial state; placeholder dashboard is a layout placeholder) |
| Empty state body | n/a |
| Error state | Invalid username or password. — Generic, does not reveal which field was wrong (per CONTEXT.md D-07 security decision) |
| Password hint text | Must be at least 6 characters. |
| Password mismatch error | Passwords do not match. |

---

## Page Layouts

## Visual Hierarchy

**SignIn page (primary screen):**
- Focal point: The full-width brand-blue "Sign In" button (`btn-primary`, #1877F2) — it's the only colored element on the card and the primary action anchor
- Scan order: Page heading ("Sign In") first → Username field → Password field → Submit button → Error message (if visible)
- The card itself is the visual container — the heading establishes context, the button resolves the action

**Setup page:**
- Focal point: The full-width brand-blue "Create Account" button
- Scan order: Heading → Description text → Username field → Password + Confirm fields → Optional fields (Name, Phone) → Submit button
- Description text and optional fields are visually de-prioritized via `text-muted` and placement below required fields

### SignIn Page (`/Auth/SignIn`)

```
┌──────────────────────────────────┐
│         Navbar (existing)         │
├──────────────────────────────────┤
│                                  │
│    ┌────────────────────────┐    │
│    │  Sign In               │    │
│    │                        │    │
│    │  [Username input     ] │    │
│    │  [Password input     ] │    │
│    │                        │    │
│    │  [  Sign In  ]  (btn)  │    │
│    │                        │    │
│    │  Error: Invalid ...    │    │
│    └────────────────────────┘    │
│                                  │
└──────────────────────────────────┘
```

- Centered card: `col-md-6 offset-md-3 col-lg-4 offset-lg-4` on md+, full width on mobile
- Card: `shadow-sm`, no border (existing `.card` with Bootstrap defaults), white background
- Card body padding: Bootstrap default (1rem / 16px)
- Heading: `<h3>` or `.h3` class, centered, `mb-3`
- Form group spacing: `mb-3` between each field
- Username: `<input type="text">`, `form-control`
- Password: `<input type="password">`, `form-control`
- Submit button: `btn btn-primary`, full width (`w-100`), min-height 48px
- Error summary: `alert alert-danger` above form OR `text-danger` below password field, 0.875rem
- No "Remember me" checkbox (v1, per CONTEXT.md)
- Brand blue (#1877F2) on the submit button via Bootstrap's `btn-primary`

### Setup Page (`/Auth/Setup`)

```
┌──────────────────────────────────┐
│         Navbar (existing)         │
├──────────────────────────────────┤
│                                  │
│    ┌────────────────────────┐    │
│    │  Set Up Admin Account  │    │
│    │                        │    │
│    │  Create the admin...   │    │
│    │                        │    │
│    │  [Username input     ] │    │
│    │  [Password input     ] │    │
│    │  [Confirm Password   ] │    │
│    │  [Name (optional)    ] │    │
│    │  [Phone (optional)   ] │    │
│    │                        │    │
│    │  [ Create Account ]    │    │
│    └────────────────────────┘    │
│                                  │
└──────────────────────────────────┘
```

- Same card layout as SignIn page
- Description paragraph below heading: `text-muted`, `mb-3`, small font
- Username: `<input type="text">`, `form-control`, required
- Password: `<input type="password">`, `form-control`, required, placeholder hint: "Must be at least 6 characters."
- Confirm Password: `<input type="password">`, `form-control`, required
- Name: `<input type="text">`, `form-control`, optional (no asterisk)
- Phone Number: `<input type="tel">`, `form-control`, optional, placeholder hint: "e.g., 09-xxx-xxxx"
- Client-side validation: Password minimum 6 chars, Passwords match
- Server-side validation errors: `asp-validation-summary` or per-field `text-danger` spans
- Submit: `btn btn-primary`, full width, min-height 48px

### Nav Bar (Updated `_Layout.cshtml`)

Same navbar structure as existing. Conditional rendering via `User.Identity.IsAuthenticated`:

**Signed out:**
```
[ServiceCity]  Home  Privacy                    Sign In
```

**Signed in:**
```
[ServiceCity]  Home  Privacy              Admin  Sign Out
```

- "Sign In" link: `<a asp-controller="Auth" asp-action="SignIn">`, text-dark
- "Admin" link: `<a asp-controller="Admin" asp-action="Dashboard">`, text-dark
- "Sign Out" link: `<a asp-controller="Auth" asp-action="SignOut">`, text-dark
- All links in `<ul class="navbar-nav">` right-aligned via `ms-auto` or separate `<ul>` with `ms-auto`
- No dropdowns, no icons — flat nav links per existing pattern
- Active state: `.active` class matches Bootstrap default (bold, slightly darker)

### Admin Dashboard Placeholder (`/Admin/Dashboard`)

```
┌──────────────────────────────────┐
│         Navbar (signed in)        │
├──────────────────────────────────┤
│                                  │
│  Admin Dashboard                 │
│  (no content — Phase 4 fills in) │
│                                  │
└──────────────────────────────────┘
```

- Minimal `<h1>Admin Dashboard</h1>` inside `<div class="container">`
- Uses existing `_Layout.cshtml` (which already has the container wrapper)
- No cards, no tables, no placeholder text — just the heading
- Phase 4 will replace this with full dashboard content

---

## Mobile-Specific Rules

| Rule | Value |
|------|-------|
| Min viewport | 360px |
| Min tap target | 48px height for all buttons and inputs |
| Form inputs full width | All inputs `w-100` on mobile (Bootstrap default for `form-control`) |
| Card edge margin | Bootstrap container padding (15px/16px each side) — sufficient at 360px |
| Navbar collapse | Existing `.navbar-expand-sm` breakpoint — hamburger menu on mobile, no change needed |

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | none | not required |
| Third-party | none | not applicable |

Bootstrap 5.3.x loaded via CDN. No npm packages, no third-party UI libraries beyond Bootstrap's own JS bundle.

---

## Verification Notes

- Client-side validation: Bootstrap 5 native validation (`form.valid` / `form.is-invalid`), no jQuery validation plugin needed (Bootstrap 5 dropped jQuery)
- Antiforgery: All POST forms include `@Html.AntiForgeryToken()` — verified by `[AutoValidateAntiforgeryToken]` in Program.cs pipeline (Phase 6 adds global enforcement; Phase 2 adds the token to forms)
- Error display: `asp-validation-summary="ModelOnly"` on both forms for server-side errors
- Setup page after admin exists: Returns 404 via controller logic — no UI to design (page is inaccessible)

---

## Checker Sign-Off

- [ ] Dimension 1 Copywriting: PASS
- [ ] Dimension 2 Visuals: PASS
- [ ] Dimension 3 Color: PASS
- [ ] Dimension 4 Typography: PASS
- [ ] Dimension 5 Spacing: PASS
- [ ] Dimension 6 Registry Safety: PASS

**Approval:** pending
