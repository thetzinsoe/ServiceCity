# 🔧 ServiceCity

> A web-based air conditioning service booking platform for Myanmar — schedule repairs, track status, get help without phone tag.

**Core Value:** Users can book AC service and know exactly when help is coming — no phone tag, no uncertainty.

---

## 📱 Screenshots

![ServiceCity Home](screenshots/01-home.png)
*Home page — browse services, book a service*

![Register](screenshots/02-register.png)
*Customer registration with phone number*

![Sign In](screenshots/03-signin.png)
*Phone number + password sign in*

![Admin Dashboard](screenshots/04-admin-dashboard.png)
*Admin dashboard — manage bookings by status*

![Booking Confirmation](screenshots/05-confirmation.png)
*Booking confirmed with reference number*

![Status Timeline](screenshots/06-status-timeline.png)
*Track booking status with visual timeline*

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 📋 **Book a Service** | Select category (Repair, Maintenance, Installation, Gas Refill), fill form, pick date/time — get a unique reference number |
| 📍 **Track Status** | Visual timeline showing booking progress: Pending → Accepted → In Progress → Completed |
| 👤 **Customer Registration** | Sign up with phone number, auto-fill booking form, manage all bookings from one place |
| 📊 **Admin Dashboard** | 5 status summary cards with counts, per-status drill-down pages, search & filter |
| ✅ **Admin Actions** | Accept with estimated arrival time, decline with reason, mark in-progress/completed |
| 🔔 **In-App Notifications** | Status updates appear on the customer's booking timeline in real-time |
| 📱 **Mobile-First** | Responsive design built for Myanmar's phone-dominant user base |

---

## 🛠 Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10 (LTS) | Application runtime |
| ASP.NET Core MVC | 10.x | Web framework — server-rendered Razor views |
| Entity Framework Core | 10.x | ORM + code-first migrations |
| PostgreSQL | 16+ | Relational database |
| Bootstrap | 5.3 | Responsive UI framework (CDN) |
| Docker | — | Containerization |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) & Docker Compose (recommended)
- PostgreSQL 16+ (if not using Docker)

### Quick Start with Docker

```bash
git clone https://github.com/thetzinsoe/ServiceCity.git
cd ServiceCity
docker compose up --build
```

App runs at **http://localhost:5124**

### Local Development

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
cd ServiceCity
dotnet run
```

### Database Setup

```bash
# Apply migrations
dotnet ef database update --project ServiceCity.Data
```

Seed data creates 4 service categories: Repair, Maintenance, Installation, Gas Refill.

---

## 📁 Project Structure

```
ServiceCity/
├── ServiceCity/                  # Web project
│   ├── Controllers/              # MVC controllers
│   │   ├── AdminController.cs    # Dashboard, actions, customers
│   │   ├── AuthController.cs     # Sign in, register, sign out
│   │   ├── BookingController.cs  # Create, status, my bookings
│   │   └── HomeController.cs     # Home page, privacy
│   ├── Views/                    # Razor views
│   │   ├── Admin/                # Dashboard, drill-down, details
│   │   ├── Auth/                 # Sign in, register, settings
│   │   ├── Booking/              # Create, status, confirmation
│   │   ├── Home/                 # Index, privacy
│   │   └── Shared/               # _Layout, _ValidationScripts
│   ├── Models/                   # View models
│   └── wwwroot/                  # Static files (CSS, JS)
│
├── ServiceCity.Core/             # Domain layer
│   ├── Entities/                 # Booking, User, Notification, ServiceCategory
│   └── Enums/                    # BookingStatus, PreferredTimeSlot
│
├── ServiceCity.Data/             # Data access layer
│   ├── AppDbContext.cs           # EF Core DbContext
│   └── Migrations/               # Database migrations
│
├── .planning/                    # Phase plans, research, roadmap
├── docker-compose.yml            # Docker orchestration
├── Dockerfile                    # Multi-stage build
└── CLAUDE.md                     # Project instructions
```

---

## 🔄 Booking Flow

```
Customer                    System                      Admin
   │                          │                           │
   ├─ Book a Service ────────►│                           │
   │                          ├─ Create booking ─────────►│
   │                          │  (SC-XXXXXXXX)            │
   │◄─ Confirmation ───────── ┤                           │
   │                          │                           ├─ Review
   │                          │◄── Accept/Decline ────────┤
   │◄─ Status update ──────── ┤                           │
   │   (timeline notification)│                           │
   │                          │◄── Start Service ─────────┤
   │◄─ Status update ──────── ┤                           │
   │                          │◄── Complete ──────────────┤
   │◄─ Status update ──────── ┤                           │
```

---

## 🏗 Architecture

- **MVC Pattern** — Controllers handle requests, Views render HTML, Models carry data
- **Service Layer** — Business logic in controllers (thin service layer for v1)
- **EF Core** — Code-first migrations, PostgreSQL via Npgsql
- **Session Auth** — Phone + name based, no email/password dependency
- **In-App Notifications** — Status/message model stored in PostgreSQL

---

## 📄 License

MIT
