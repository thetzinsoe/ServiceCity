# ServiceCity — Proposal by @thetzinsoe

## Gist
A web-based air conditioning service booking platform where users can schedule repairs and maintenance with a single service shop, and the admin can accept, decline, or schedule visit times with in-app notifications.

## Story
**Ko Min** runs a small electronics shop in Yangon. His office AC unit breaks down in the middle of hot season. He has to call around different service numbers, describe the problem over and over, and wait for someone to show up — maybe today, maybe next week. There's no way to track if his request was received or when help is coming. He opens ServiceCity on his phone browser, picks the issue from a list (repair, maintenance, gas refill), enters his name and phone number, and books. When the admin accepts, he gets an in-app notification with the technician's estimated arrival time. No phone tag, no uncertainty.

## Why
AC repair booking in Myanmar still runs on phone calls and Facebook messages — it's slow, unorganized, and frustrating for both customers and service providers. ServiceCity removes the back-and-forth by giving users a simple booking flow and giving the admin a dashboard to manage requests and schedules. The app uses in-app notifications because email usage is low in Myanmar, and phone-based account setup keeps the barrier to entry minimal.

## Why Not
<!-- What you are deliberately NOT building. Scope walls. 2-3 bullets. (Ch3) -->
- Not a multi-shop marketplace in v1 — only one admin/service shop manages all bookings
- No online payments in v1 — payment is handled on-site when the technician arrives
- No mobile app in v1 — fully responsive web app first; mobile app is a future version
- No user reviews or ratings in v1 — this will be added in a later version

## Tech Spec
- **Stack:** ASP.NET Core MVC (.NET 10) + Entity Framework Core + PostgreSQL (via Npgsql)
- **Frontend:** Razor views + Bootstrap 5 (responsive, mobile-first for Myanmar users)
- **Auth:** Phone number + name based session (no email/password in v1 — keeps it simple)
- **Notifications:** In-app notification system via a status/message model in the database; the user sees updates on their booking page
- **Hosting:** Deployed via Docker container (TBD cloud provider)
- **Architecture:** Standard MVC pattern with service layer for business logic, repository pattern for data access, and EF Core migrations for schema management

## Definition of Done
- [ ] A user can open ServiceCity in a web browser on mobile or desktop
- [ ] A user can select a service category (repair / maintenance / installation / gas refill) and book without creating an account — just name + phone
- [ ] The admin can see all bookings in a dashboard, sorted by status (pending / accepted / declined)
- [ ] The admin can accept a booking and set the estimated arrival time
- [ ] The admin can decline a booking with a reason
- [ ] When the admin accepts or declines, the user sees the status update on their booking page (in-app notification)
- [ ] The user can check their booking status using their phone number
- [ ] The database (PostgreSQL) stores users, bookings, service categories, and status history
