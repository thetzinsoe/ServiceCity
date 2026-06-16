# Feature Research

**Domain:** AC service booking platform (single-shop, Myanmar market)
**Researched:** 2026-06-16
**Confidence:** HIGH

## Feature Landscape

### Table Stakes (Users Expect These)

Features users assume exist. Missing these = product feels incomplete.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Service category selection | Users need to indicate what kind of AC work they need (repair, maintenance, installation, gas refill, cleaning) so the shop can prepare | LOW | Dropdown or card-based selection. Pre-populate 4-6 common AC service types. |
| Booking form (name + phone + address + problem description) | Core data capture to process a service request. Name and phone are the minimum identity in Myanmar. | LOW | Phone input should accept Myanmar numbers (09-xxxxxxxxx). Address can be free text or structured. |
| Preferred date/time selection | Users want to tell the shop when they are available. Reduces phone tag. | MEDIUM | Date picker + time slots (morning/afternoon/evening). NOT real-time availability — admin manually confirms. |
| Booking confirmation page with reference number | After submitting, users need to know the booking was received. A reference number lets them check status later. | LOW | Generate a short human-readable booking ID (e.g., SC-00125). Show on confirmation screen. |
| Booking lookup by phone number | Users in Myanmar do not use email. Phone number is their identity. They need to check "what happened to my booking?" | LOW | Simple form: enter phone, see booking list. No account needed. |
| Status page per booking | Users expect to see whether their booking is pending, accepted, in-progress, or completed. This replaces the anxiety of "did they get my request?" | MEDIUM | Statuses: Pending → Accepted → In Progress → Completed. Also: Declined. Each status shows timestamp and admin notes. |
| Admin dashboard: booking list sorted by status | The admin needs a single view of all incoming work. This replaces the phone/Facebook-message chaos. | MEDIUM | Table or card list. Default sort: newest first. Filter by status (Pending, Accepted, In Progress, Completed, Declined). |
| Admin: accept booking + set estimated arrival time | Core workflow: admin reviews a booking, accepts it, and tells the user when to expect the technician. | MEDIUM | Accept action opens a form to set estimated date/time window. Saves to booking. |
| Admin: decline booking with reason | Not all bookings can be served (out of area, no parts, over capacity). Admin needs a way to say no with an explanation. | LOW | Decline action opens a text field for reason. User sees the reason on their status page. |
| Admin: mark booking as in-progress and completed | Technicians start work and finish work. Admin updates status so the user knows what's happening. | LOW | Simple status transitions. Completed status triggers "service done" display for user. |
| In-app status notification when user views their booking | Since email is unreliable and SMS costs money, in-app notifications on the booking status page are the primary notification channel. | LOW | Status changes are displayed when the user checks their booking. A "new update" indicator/badge on the booking list. |
| Mobile-responsive design | Myanmar users access the internet almost exclusively via smartphones. The entire UI must work on small screens. | LOW | Bootstrap 5 responsive grid. Test on 320px width. Touch-friendly buttons (min 44px tap target). |
| Burmese language number formatting and date display | Myanmar users understand dates and numbers in Burmese format. English-only dates alienate users. | LOW | Use Burmese month names, Myanmar number system, or at minimum DD/MM/YYYY format. |
| Admin login (username + password) | The admin needs a secure way to access the dashboard. Different from user phone auth. | MEDIUM | Standard ASP.NET Core Identity or simple credential check. Admin is the single shop owner/staff. |

### Differentiators (Competitive Advantage)

Features that set the product apart. Not required, but valuable.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Phone-only identity (zero friction booking) | In Myanmar, asking for email or password is a conversion killer. Name + phone is all that's needed. Competing with phone calls means the booking must be faster than dialing. | LOW | This is already in the requirements. The differentiator is the discipline to NOT add registration friction later. |
| Quick rebooking for returning phone numbers | Returning customers shouldn't re-enter their name and address every time. Recognizing their phone number builds loyalty without an account system. | MEDIUM | When a phone number is entered, check existing bookings. Auto-fill name and last address. Show "Book again?" prompt. |
| Admin cost estimate before technician visit | When the admin accepts a booking, they can optionally provide an estimated cost range. This reduces customer anxiety and no-shows. Competitors rarely do this. | LOW | Free-text or numeric field on the Accept form. Displayed as "Estimated: 50,000 - 80,000 MMK." Clearly labeled as estimate. |
| Photo upload of AC unit or problem | Users can snap a photo of their AC unit, model label, or visible problem. This helps the admin diagnose before sending a technician (right parts, right tools). | MEDIUM | Camera/file upload input on booking form. Store images in server filesystem or blob. Limit file size to 5MB. Optional field. |
| "Technician is on the way" status with SMS notification | When the admin dispatches the technician, the user gets an SMS. This is the ONE notification that justifies SMS cost because it prevents missed appointments. | MEDIUM | Integrate with a Myanmar SMS gateway (e.g., Ooredoo MEC, MPT SMS API). Triggered when admin sets status to "In Progress" or "On the Way." |
| Service history by phone number | Users can see their past bookings when they look up by phone. This builds trust and makes rebooking feel familiar. | LOW | Query bookings by phone, show chronological list. Simple read-only view. |
| Admin calendar/schedule view | Beyond the list view, the admin can see bookings on a calendar (day/week view). Helps with technician scheduling and capacity planning. | MEDIUM | Use a JS calendar library (e.g., FullCalendar). Show accepted bookings with time slots. Optional in v1, high value for admin. |
| Dark mode for admin dashboard | Admin may work late hours managing bookings. Dark mode reduces eye strain and feels premium. | LOW | Bootstrap 5.3+ has built-in dark mode support. Toggle in admin nav. |
| Offline-capable booking form (PWA basics) | Myanmar mobile networks can be unreliable. A service worker that caches the booking form means users can start a booking even with spotty internet. | HIGH | PWA service worker + cache strategy. Significant complexity. Defer to v1.x — not v1. |
| Burmese language UI (full localization) | The majority of Myanmar users are more comfortable in Burmese. Full localization removes the language barrier completely. | MEDIUM | ASP.NET Core localization with .resx files. Start with key screens (booking form, status page). Expand incrementally. |

### Anti-Features (Commonly Requested, Often Problematic)

Features that seem good but create problems.

| Feature | Why Requested | Why Problematic | Alternative |
|---------|---------------|-----------------|-------------|
| Email-based registration and notifications | "Every app has email login." | Email usage in Myanmar is extremely low (below 15%). Requiring email will kill conversion. Email delivery to Myanmar ISPs is unreliable. | Phone-only identity. In-app status page as notification channel. Optional SMS for critical updates. |
| Real-time technician GPS tracking | "Uber-like tracking is expected." | Requires mobile app on technician's phone, constant GPS polling, map integration, and reliable data connection. Massive complexity for a single-shop with 2-3 technicians. | "Technician is on the way" status with estimated arrival time. SMS notification when dispatched. |
| Multi-vendor marketplace | "More shops = more choice for users." | Introduces shop onboarding, quality control, commission logic, search ranking, conflict resolution, and trust/safety concerns. Doubles or triples scope. | Single-shop model validated first. If the model works for one shop, marketplace can be a v2 decision with real data. |
| Online payment integration | "Users want to pay online." | Requires payment gateway integration (KBZ Pay, WavePay), PCI compliance, refund logic, dispute handling, and constant gateway API maintenance. Adds months to timeline. | On-site payment (cash or mobile transfer when technician arrives). Standard practice in Myanmar AC service. Document as out-of-scope for v1. |
| User reviews and star ratings | "Reviews build trust." | Premature for a single-shop. Reviews have no comparison value without multiple shops. Negative reviews hurt the shop with no recourse. Moderation burden. | Defer until marketplace v2. For v1, focus on service quality and word-of-mouth. Consider a simple "Was the service completed?" confirmation instead. |
| Chat / messaging between user and admin | "Real-time communication is better." | Adds moderation burden, response-time expectations, message storage, and UI complexity. Users already have the admin's phone number for calls. | Status updates + admin notes on bookings cover 90% of communication needs. Users can call for urgent issues. |
| Social media login (Facebook, Google) | "One-click login is convenient." | Adds OAuth dependency and complexity. Facebook API changes break login. Users may not want to link social accounts. Phone number is simpler and universal in Myanmar. | Phone-only identity. No login needed for users at all — just enter phone to look up bookings. |
| AI-powered diagnosis from problem description | "Smart features attract users." | AC diagnosis requires visual inspection and technical testing. Text-based AI diagnosis will be inaccurate and create false expectations. Liability risk if diagnosis is wrong. | Photo upload for visual context. Admin manually reviews and prepares. |
| Automated scheduling / slot booking | "Users should pick exact time slots." | Real-time slot management requires knowing technician availability, travel time, job duration — all variables the admin controls manually. Automated slots create double-bookings. | Preferred date + time preference (morning/afternoon). Admin manually confirms exact time on acceptance. |
| Push notifications (mobile app) | "Users expect push notifications." | Requires a native mobile app, FCM/APNs integration, and permission management. The project is web-first. | In-app status page with update badges. SMS for the one critical notification (technician dispatched). PWA push notifications as a future enhancement. |
| Customer membership / loyalty points | "Retain customers with points." | Premature optimization. Single shop with limited capacity doesn't need a loyalty program. Adds database complexity and redemption logic. | Quick rebooking for returning phone numbers. Good service is the loyalty program. |

## Feature Dependencies

```
Booking Lookup by Phone
    └──requires──> Booking Form (name + phone + address)
                       └──requires──> Service Category Selection
                       └──requires──> Preferred Date/Time Selection

Status Page per Booking
    └──requires──> Booking Confirmation (reference number)
    └──requires──> Admin: Accept/Decline Booking
                       └──requires──> Admin Dashboard: Booking List
                       └──requires──> Admin Login

In-App Status Notification (badge)
    └──requires──> Status Page per Booking
    └──requires──> Admin: mark as in-progress/completed

Quick Rebooking
    └──requires──> Booking Form (name + phone)
    └──enhances──> Booking Lookup by Phone

Technician on the Way SMS
    └──requires──> Admin: mark as in-progress
    └──requires──> SMS gateway integration

Admin Calendar View
    └──enhances──> Admin Dashboard: Booking List
    └──requires──> Admin: accept booking + set estimated time

Photo Upload
    └──enhances──> Booking Form
    └──requires──> File storage (server filesystem or blob)

Burmese Localization
    └──enhances──> All user-facing features
    └──requires──> ASP.NET Core localization setup (resx files)
```

### Dependency Notes

- **Booking Lookup depends on Booking Form:** Can't look up what hasn't been created. The booking form must exist and store data before lookup works.
- **Status Page depends on Admin Accept/Decline:** The status page shows admin actions. Without admin workflow, the status page is static "Pending" forever.
- **Admin Calendar depends on estimated arrival times:** The calendar view is only useful if bookings have time data. Calendar should come after the basic accept-with-time workflow is solid.
- **Quick Rebooking enhances Booking Lookup:** Rebooking uses the same phone-number identity. Implement lookup first, then add auto-fill and rebooking prompt.
- **Photo Upload is standalone:** It enhances the booking form but has no hard dependencies. Can be added anytime after the booking form exists.

## MVP Definition

### Launch With (v1)

Minimum viable product — what's needed to validate the concept.

- [ ] **Service category selection** — Core differentiator from "just call." Users pick what they need.
- [ ] **Booking form (name + phone + address + problem description)** — The entire user-side data capture.
- [ ] **Preferred date/time selection** — Reduces phone tag. Users indicate availability.
- [ ] **Booking confirmation with reference number** — Users know the booking was received.
- [ ] **Booking lookup by phone number** — Users check status without an account.
- [ ] **Status page per booking** — Users see pending/accepted/in-progress/completed/declined.
- [ ] **Admin login** — Secure dashboard access.
- [ ] **Admin dashboard: booking list sorted by status** — The admin's main workspace.
- [ ] **Admin: accept booking + set estimated arrival time** — Core admin workflow.
- [ ] **Admin: decline booking with reason** — Necessary for bookings that can't be served.
- [ ] **Admin: mark booking as in-progress and completed** — Complete the service lifecycle.
- [ ] **In-app status notification (badge/indicator)** — Users see when there's a new update.
- [ ] **Mobile-responsive design** — Non-negotiable for Myanmar users on smartphones.

### Add After Validation (v1.x)

Features to add once core is working.

- [ ] **Admin cost estimate on acceptance** — Reduces customer anxiety. Add when basic accept workflow is stable and admin requests it.
- [ ] **Quick rebooking for returning phone numbers** — Add after 50+ bookings when repeat customers emerge.
- [ ] **Technician on the way SMS notification** — Add when missed-appointment rate is measured and justifies SMS cost.
- [ ] **Photo upload of AC unit/problem** — Add when admin reports needing visual context before visits.
- [ ] **Burmese language UI (booking form + status page)** — Add when user feedback indicates language is a barrier. Start with key screens.
- [ ] **Admin calendar/schedule view** — Add when admin has enough daily bookings that a list view becomes hard to manage.

### Future Consideration (v2+)

Features to defer until product-market fit is established.

- [ ] Multi-shop marketplace — Only if the single-shop model is successful and other shops request onboarding.
- [ ] Online payments (KBZ Pay, WavePay) — Only if users consistently request it and payment infrastructure matures.
- [ ] PWA offline support — Only if network reliability data justifies the complexity.
- [ ] Native mobile app — Only if web analytics show sufficient mobile traffic and users request app-specific features (push notifications, camera integration).
- [ ] User reviews and ratings — Only in a multi-shop context where comparison is meaningful.
- [ ] Chat/messaging — Only if status updates prove insufficient for communication needs.
- [ ] Customer loyalty program — Only at significant scale with repeat purchase data.

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Service category selection | HIGH | LOW | P1 |
| Booking form (name + phone + address + problem) | HIGH | LOW | P1 |
| Preferred date/time selection | HIGH | MEDIUM | P1 |
| Booking confirmation with reference number | HIGH | LOW | P1 |
| Booking lookup by phone number | HIGH | LOW | P1 |
| Status page per booking | HIGH | MEDIUM | P1 |
| Admin login | HIGH | MEDIUM | P1 |
| Admin dashboard: booking list | HIGH | MEDIUM | P1 |
| Admin: accept + set arrival time | HIGH | MEDIUM | P1 |
| Admin: decline with reason | MEDIUM | LOW | P1 |
| Admin: mark in-progress/completed | HIGH | LOW | P1 |
| In-app status notification badge | MEDIUM | LOW | P1 |
| Mobile-responsive design | HIGH | LOW | P1 |
| Burmese date/number formatting | MEDIUM | LOW | P1 |
| Admin cost estimate on acceptance | MEDIUM | LOW | P2 |
| Quick rebooking (returning phone) | MEDIUM | MEDIUM | P2 |
| Technician on the way SMS | HIGH | MEDIUM | P2 |
| Photo upload of AC unit/problem | MEDIUM | MEDIUM | P2 |
| Burmese language full UI | MEDIUM | MEDIUM | P2 |
| Admin calendar/schedule view | MEDIUM | MEDIUM | P2 |
| Dark mode (admin dashboard) | LOW | LOW | P3 |
| PWA offline support | MEDIUM | HIGH | P3 |
| Multi-shop marketplace | HIGH | HIGH | P3 |
| Online payments | MEDIUM | HIGH | P3 |
| User reviews and ratings | LOW | MEDIUM | P3 |
| Chat/messaging | LOW | HIGH | P3 |
| Native mobile app | MEDIUM | HIGH | P3 |

**Priority key:**
- P1: Must have for launch
- P2: Should have, add when possible
- P3: Nice to have, future consideration

## Competitor Feature Analysis

| Feature | Urban Company (India) | Local Myanmar (FB/Phone) | Our Approach |
|---------|----------------------|--------------------------|--------------|
| Service categories | 20+ categories with sub-services | "What's wrong?" over phone | 4-6 focused AC categories, clearly labeled |
| Identity | Phone OTP or Google login | Caller ID + memory | Name + phone only, no OTP, no account |
| Booking flow | Multi-step wizard with upsells | Verbal description | Single-page form, minimal fields |
| Scheduling | Real-time slot booking with availability | "When are you free?" negotiation | Preferred date/time, admin manually confirms |
| Payment | Online prepayment + UPI | Cash on completion | Cash on completion (no payment in app) |
| Notifications | Push + email + SMS | Phone call | In-app status page + optional SMS for dispatch |
| Tracking | Live technician GPS map | "Where are you?" phone call | Status updates + estimated arrival time |
| Reviews | 5-star ratings + photos | Word of mouth | None in v1 — focus on service quality |
| Admin tools | Partner app + CRM dashboard | Pen + paper / mental notes | Web dashboard with status management |
| Pricing | Dynamic pricing, surge, discounts | Negotiated per job | Fixed price per category (or admin-set estimate) |
| Language | English + Hindi | Burmese | Burmese date/numbers in v1, full localization in v1.x |

## Myanmar Market Considerations

These are assumptions about the target market that inform feature decisions. Validate with real users.

| Assumption | Impact on Features | Validation Method |
|------------|-------------------|-------------------|
| Smartphone penetration is high but almost entirely Android | Test primarily on Chrome for Android. Bootstrap 5 handles this. | Analytics after launch |
| Internet connectivity can be slow or intermittent | Minimize page weight. No heavy JS frameworks (Razor views are server-rendered, lightweight). | Monitor page load times |
| Facebook Messenger is widely used for business communication | Facebook integration is NOT a v1 requirement, but the booking link can be shared via Facebook. A "Share via Messenger" link is low effort. | Observe whether users arrive from Facebook links |
| Myanmar phone numbers follow 09-xxxxxxxxx pattern | Phone input validation must accept this format. | Test with real numbers |
| Users expect free or very cheap SMS | Use SMS sparingly (only the "technician dispatched" notification). SMS costs in Myanmar are non-trivial. | Research SMS gateway pricing before implementing |
| Hot season (March-May) drives peak AC service demand | The platform should handle seasonal spikes. No special features needed in v1 — just ensure the stack doesn't break under load. | Load test before hot season |
| Cash is dominant, mobile payments growing but not universal | No payment integration needed. On-site cash collection is the norm and expectation. | Monitor KBZ Pay / WavePay adoption over time |

## Sources

- Domain knowledge of home service booking platforms (Urban Company, TaskRabbit, Thumbtack, Housejoy)
- Myanmar market context: phone-first internet usage, low email penetration, dominant Android platform
- PROJECT.md requirements and constraints for ServiceCity
- Standard admin dashboard and CRUD application patterns
- Bootstrap 5 mobile-first responsive design patterns
- ASP.NET Core MVC + Razor views server-rendered architecture

---
*Feature research for: AC service booking platform (ServiceCity)*
*Researched: 2026-06-16*
