---
phase: 04
slug: admin-dashboard
status: implemented
shadcn_initialized: false
preset: none
created: 2026-06-19
updated: 2026-06-20
---

# Phase 04 — UI Design Contract

> Visual and interaction contract for Phase 04: Admin Dashboard. Generated from upstream artifacts (REQUIREMENTS.md, ROADMAP.md) and verified against implemented code.
>
> **Status:** Implemented — this spec reflects the as-built state (commits up to `6827f93`).

---

## Design System

| Property | Value |
|----------|-------|
| Tool | none (Bootstrap 5.3.x CDN, server-rendered Razor views) |
| Preset | not applicable |
| Component library | Bootstrap 5.3.x (via CDN) |
| Icon library | Emoji (🟡🔵🟠🟢🔴🔧🧹📦❄️📅🕐📍📞📝👤🔍) |
| Font | Bootstrap default system font stack |

---

## Spacing Scale

Bootstrap 5 defaults (multiples of 4). Explicit tokens for dashboard layout:

| Token | Value | Usage |
|-------|-------|-------|
| xs | 4px | — |
| sm | 8px | Gap between filter pills (`gap-2`) |
| md | 16px | Card grid gutters (`g-3` = 16px), card body padding |
| lg | 24px | Section spacing between status groups (`mb-4`) |
| xl | 32px | — |
| 2xl | 48px | — |
| 3xl | 64px | — |

---

## Typography

| Role | Size | Weight | Line Height |
|------|------|--------|-------------|
| Body | 1rem (16px) | 400 | 1.5 |
| Page heading | 1.5rem (~24px, `.h4`) | 700 (`fw-bold`) | 1.3 |
| Card title (customer name) | 1.25rem (~20px, `.h5`) | 700 (`fw-bold`) | 1.3 |
| Section header (status) | 1.25rem (~20px, `fs-5`) | 700 (`fw-bold`) | 1.3 |
| Detail label | 0.875rem (14px) | 400 | 1.5 |
| Small / meta text | 0.875rem (14px, `small`) | 400 | 1.5 |
| Badge text | 1rem (16px, `fs-6`) | 400 | 1.5 |

---

## Color

| Role | Value | Usage |
|------|-------|-------|
| Dominant (60%) | #FFFFFF | Page background, card background |
| Secondary (30%) | #F0F2F5 | Card grouping surfaces (when used) |
| Accent (10%) | #1877F2 | Primary CTA buttons, active nav links, active filter pill, navbar brand |
| Pending | bg: #FFF3CD, text: #856404 | Pending status badge, section header |
| Accepted | bg: #CCE5FF, text: #004085 | Accepted status badge, section header |
| In Progress | bg: #FFF3CD, text: #856404 | In Progress status badge |
| Completed | bg: #D4EDDA, text: #155724 | Completed status badge, section header |
| Declined | bg: #F8D7DA, text: #721C24 | Declined status badge, section header |
| New (today) | #DC3545 | "New" badge on today's bookings |
| Success action | #28A745 | Accept button, Complete button |
| Warning action | #FFC107 | Start Service button |
| Destructive | #DC3545 | Decline button, Sign Out link |

Accent (#1877F2) reserved for: primary buttons, active nav links, active filter pills, navbar brand color. Used sparingly — only one accent element per component.

---

## Copywriting Contract

| Element | Copy |
|---------|------|
| Dashboard page heading | Bookings |
| Total/pending badge | `{N} total · {N} pending` |
| Filter — All | All |
| Filter — Pending | Pending |
| Filter — In Progress | In Progress |
| Filter — Completed | Completed |
| Filter — Declined | Declined |
| Search placeholder | Search by phone, reference number, or customer name… |
| Search button | Search |
| Clear search link | Clear |
| Section header — Pending | 🟡 Pending |
| Section header — Accepted | 🔵 Accepted |
| Section header — In Progress | 🟠 InProgress |
| Section header — Completed | 🟢 Completed |
| Section header — Declined | 🔴 Declined |
| "New" badge | New |
| Empty section | No bookings in this status. |
| Global empty state heading | No bookings yet. |
| Global empty state body | Bookings will appear here when customers submit them. |
| Detail — Back link | ← Back to Bookings |
| Detail — Customer card label | CUSTOMER |
| Detail — Service card label | SERVICE |
| Detail — Actions label | ACTIONS |
| Detail — Accept label | Estimated Arrival Time |
| Detail — Accept button | Accept |
| Detail — Decline label | Decline Reason |
| Detail — Decline placeholder | Why? |
| Detail — Decline button | Decline |
| Detail — Start Service button | Start Service |
| Detail — Complete button | Complete Job |
| Detail — Decline reason label | DECLINED |
| Detail — Timeline label | TIMELINE |
| Detail — In Progress action text | Technician is on site — mark complete when done. |
| Nav — Authenticated | All Bookings |
| Nav — Profile dropdown header | Signed in as {Name} |
| Nav — Profile Settings | ⚙️ Settings |
| Nav — Profile Sign Out | Sign Out |
| Nav — Unauthenticated | Sign In |

---

## Page Layouts

### Visual Hierarchy

**Dashboard (primary screen):**
- Focal point: Status filter pills and search bar together form the interaction zone
- Scan order: Heading + total badge → Status filter pills → Search bar → Status sections (Pending first) → Booking cards
- Active filter pill is filled brand-blue (#1877F2); inactive pills are outline-secondary

**Detail page:**
- Focal point: Reference number + status badge at top
- Scan order: Back link → Ref number + badge → Customer/Service cards (side-by-side) → Actions card → Timeline

### Dashboard Page (`/Admin/Dashboard`)

```
┌──────────────────────────────────────────────────┐
│              Navbar (signed in)                    │
│  [🔧 ServiceCity]  Home  Book a Service ▾  All Bookings    👤 Admin ▾  │
├──────────────────────────────────────────────────┤
│                                                    │
│  Bookings                        12 total · 3 pending  │
│                                                    │
│  [All] [Pending] [In Progress] [Completed] [Declined] │
│                                                    │
│  ┌──────────────────────────────────────┐         │
│  │ 🔍  Search by phone, reference...  [Search] │         │
│  └──────────────────────────────────────┘         │
│                                                    │
│  🟡 Pending  ┌────┐                               │
│              │ 3  │                               │
│              └────┘                               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐          │
│  │ [New]    │ │          │ │          │          │
│  │ Ma Ma    │ │ Ko Ko    │ │ Aye Aye  │          │
│  │ Repair   │ │ Maint.   │ │ Install  │          │
│  │ 📅 20/06 │ │ 📅 19/06 │ │ 📅 18/06 │          │
│  │ 🕐 AM    │ │ 🕐 PM    │ │ 🕐 Eve   │          │
│  │ SC-0001  │ │ SC-0002  │ │ SC-0003  │          │
│  └──────────┘ └──────────┘ └──────────┘          │
│                                                    │
│  🔵 Accepted  ┌────┐                              │
│               │ 4  │                              │
│               └────┘                              │
│  ┌──────────┐ ┌──────────┐ ...                   │
│  │ ...      │ │ ...      │                       │
│  └──────────┘ └──────────┘                       │
│                                                    │
│  ... (remaining status sections)                   │
│                                                    │
└──────────────────────────────────────────────────┘
```

- Heading: `.h4.fw-bold`, left-aligned
- Total badge: `badge bg-primary fs-6`, right-aligned opposite heading
- Filter pills: `<div class="status-filter mb-3">` with pill-style buttons
  - Active pill: `btn btn-sm btn-primary` (filled #1877F2)
  - Inactive pill: `btn btn-sm btn-outline-secondary`
  - Pills use `border-radius: 20px`, `font-weight: 500`
  - Mobile: `min-height: 48px` on small screens
  - Each pill is an `<a>` tag linking to `?status=X&search=Y` (preserving search)
  - "All" pill links without status param (clears filter)
- Search form: GET to `Dashboard`, `mb-4`
  - Hidden `<input type="hidden" name="status">` when filter active (preserves filter on search submit)
  - Input group: search icon span + text input + optional "Clear" link + "Search" button
  - Clear link preserves active status filter: `asp-route-status="@Model.StatusFilter"`
- Status sections: rendered in order (Pending → Accepted → InProgress → Completed → Declined)
  - Section header: emoji icon + status name (`fw-bold fs-5`) + count badge (`badge rounded-pill` with status color)
  - When StatusFilter is set, only the matching section renders; others are skipped
  - Booking cards: responsive grid `row-cols-1 row-cols-md-2 row-cols-lg-3 g-3`
- Booking cards: `<a>` wrapper linking to `Admin/Details/{id}`
  - Card: `card booking-card h-100` with shadow-sm, border-0
  - "New" badge: red (#DC3545), positioned top-left absolute, appears when `CreatedAt.Date == today`
  - Card body: Customer name (card-title fw-bold) + status badge + category name + date/time + reference number
  - Hover: lift effect via CSS (`transform: translateY(-2px)`, enhanced box-shadow)
- Empty global state (when no bookings exist): centered text "No bookings yet." + subtext

### Detail Page (`/Admin/Details/{id}`)

```
┌──────────────────────────────────────────────────┐
│              Navbar (signed in)                    │
├──────────────────────────────────────────────────┤
│                                                    │
│  ← Back to Bookings                               │
│                                                    │
│  SC-00000001           ┌──────────────┐           │
│                        │   Pending    │           │
│                        └──────────────┘           │
│                                                    │
│  ┌─────────────────┐ ┌─────────────────┐          │
│  │ CUSTOMER         │ │ SERVICE          │          │
│  │                  │ │                  │          │
│  │ Ma Ma            │ │ 🔧 Repair        │          │
│  │ 📞 09-xxx-xxxx  │ │ 📅 20/06/2026    │          │
│  │ 📍 Yangon       │ │ 🕐 AM            │          │
│  │                  │ │ 📝 AC not cool   │          │
│  │                  │ │ Created 18/06... │          │
│  └─────────────────┘ └─────────────────┘          │
│                                                    │
│  ┌──────────────────────────────────────┐         │
│  │ ACTIONS                              │         │
│  │                                      │         │
│  │ Est. Arrival Time:                   │         │
│  │ [datetime picker...]    [Accept]     │         │
│  │                                      │         │
│  │ Decline Reason:                      │         │
│  │ [Why?..............] [Decline]       │         │
│  └──────────────────────────────────────┘         │
│                                                    │
│  TIMELINE                                          │
│  ● Booking created — Pending                       │
│  │  18/06/2026 02:30 PM                            │
│  ● Status changed to Accepted                      │
│    19/06/2026 09:15 AM                             │
│                                                    │
└──────────────────────────────────────────────────┘
```

- Back link: `text-muted text-decoration-none small` with ← arrow
- Heading row: reference number (`h4 fw-bold`) + status badge (`badge fs-6 px-3 py-2 rounded-pill` with status colors)
- Two-column layout (`row g-3`): Customer card (col-md-6) + Service card (col-md-6)
  - Each card: label heading (`text-muted small text-uppercase fw-bold mb-3`) + detail content
  - Customer: name (`fw-bold fs-5`), phone (📞), address (📍)
  - Service: category with emoji icon, date (📅), time slot (🕐), optional description (📝), created date
- Actions card (status-dependent):
  - **Pending:** Two rows — Accept form (datetime-local input + green "Accept" button) and Decline form (text input with placeholder "Why?" + outline-danger "Decline" button)
  - **Accepted:** Shows arrival time + yellow "Start Service" button
  - **InProgress:** Shows "Technician is on site" text + green "Complete Job" button
  - **Declined:** Shows decline reason in red text on light background
  - All forms include `@Html.AntiForgeryToken()` and hidden `id` field
- Timeline: emoji-free dot + vertical line + message + timestamp
  - Dot color matches status: Pending=#FFC107, Accepted=#1877F2, InProgress=#FD7E14, Completed=#28A745, Declined=#DC3545
  - Vertical line via `.vr` between dots (hidden on last item)

---

## Mobile-Specific Rules

| Rule | Value |
|------|-------|
| Min viewport | 360px |
| Min tap target | 48px height for all buttons, filter pills, and form inputs |
| Card grid | `row-cols-1` on mobile (single column), expands to `row-cols-md-2 row-cols-lg-3` |
| Filter pills | Wrap naturally via `flex-wrap`; `min-height: 48px` on screens <576px |
| Status badge on detail | Full-width below reference number on narrow screens (`mb-2 mb-md-0`) |
| Detail cards | Stack vertically on mobile (`col-md-6`), side-by-side on md+ |
| Action forms | Stack vertically on mobile, side-by-side on md+ (`col-md-7` + `col-md-5`) |
| Search input | Full-width with icon; Clear and Search buttons remain inline |
| Navbar | Collapses at `sm` breakpoint (`navbar-expand-md`) |

---

## Nav Bar (Updated `_Layout.cshtml`)

**Signed out:**
```
[🔧 ServiceCity]  Home  Book a Service ▾            Sign In
```

**Signed in:**
```
[🔧 ServiceCity]  Home  Book a Service ▾  All Bookings            👤 Admin ▾
```

- "All Bookings" link: `<a asp-controller="Admin" asp-action="Dashboard">`, visible only when authenticated
- "Check Status" nav item: **REMOVED** (was a public link to `Booking/Lookup`)
- Profile dropdown (right-aligned): "Signed in as {Name}" header → Settings → divider → Sign Out (red)
- No dropdowns, no icons on main nav — flat nav links per existing pattern
- Active state: `.active` class matches Bootstrap default (bold, slightly darker)

---

## Registry Safety

| Registry | Blocks Used | Safety Gate |
|----------|-------------|-------------|
| shadcn official | none | not required |
| Third-party | none | not applicable |

Bootstrap 5.3.x loaded via CDN. htmx 2.0 loaded via unpkg CDN (for future partial updates). No npm packages, no third-party UI libraries beyond Bootstrap's own JS bundle.

---

## Verification Notes

- Status filter: URL query param composes with search — `?status=Pending&search=09` filters by both
- Clear search: preserves active status filter via `asp-route-status`
- Search submission: hidden status field preserves filter across searches
- When no filter active: all status sections render (same as before filter was added)
- When filter active: only matching section renders (others skipped via `continue`)
- "All" link: omits status param entirely, returning to unfiltered view
- Booking cards: `text-decoration-none` on the `<a>` wrapper keeps cards clean
- "New" badge: timezone-aware check (`CreatedAt.Date == DateTime.UtcNow.Date`) — shows for bookings created today UTC
- Antiforgery: All POST forms on Detail page include `@Html.AntiForgeryToken()`
- Responsive: Cards collapse from 3→2→1 columns; filter pills wrap; detail stacks vertically

---

## Checker Sign-Off

- [ ] Dimension 1 Copywriting: PASS
- [ ] Dimension 2 Visuals: PASS
- [ ] Dimension 3 Color: PASS
- [ ] Dimension 4 Typography: PASS
- [ ] Dimension 5 Spacing: PASS
- [ ] Dimension 6 Registry Safety: PASS

**Approval:** pending
