# Architecture Research

**Domain:** Service booking platform (AC repair/maintenance)
**Researched:** 2026-06-16
**Confidence:** MEDIUM

> Training-data based. No web verification was possible in this environment. ASP.NET Core MVC patterns are stable and well-documented up to .NET 10. Key claims align with project constraints in PROJECT.md. Architecture choices are opinionated for this specific scope.

## Standard Architecture

### System Overview — N-Layer with Service Layer

ServiceCity uses a pragmatic **4-layer architecture**. Full Clean Architecture (with domain/infrastructure separation) is overkill for a single-shop booking system at v1 scale. The layers are thin enough to refactor later if the system grows into a marketplace.

```
┌─────────────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │  Razor Views  │  │ Controllers   │  │  ViewModels / DTOs      │  │
│  │  (Bootstrap 5)│  │ (thin,        │  │  (shape data for views) │  │
│  │               │  │  delegate to  │  │                         │  │
│  │               │  │  services)    │  │                         │  │
│  └──────┬───────┘  └──────┬───────┘  └────────────┬─────────────┘  │
│         │                 │                         │               │
├─────────┴─────────────────┴─────────────────────────┴───────────────┤
│                    APPLICATION LAYER                                 │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Service Classes                            │   │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  │   │
│  │  │ BookingService │  │ AdminService   │  │ NotificationSvc│  │   │
│  │  │ - Create       │  │ - ListPending  │  │ - Create       │  │   │
│  │  │ - Lookup       │  │ - Accept       │  │ - GetForBooking│  │   │
│  │  │ - StatusFlow   │  │ - Decline      │  │                │  │   │
│  │  └───────┬────────┘  └───────┬────────┘  └───────┬────────┘  │   │
│  │          │                   │                    │           │   │
│  │  ┌───────┴───────────────────┴────────────────────┴────────┐  │   │
│  │  │                  SessionService                          │  │   │
│  │  │  - SignIn(phone, name)  - GetCurrentUser()              │  │   │
│  │  └──────────────────────────┬──────────────────────────────┘  │   │
│  └─────────────────────────────┼─────────────────────────────────┘   │
│                                 │                                     │
├─────────────────────────────────┼─────────────────────────────────────┤
│                    DATA ACCESS LAYER                                  │
│  ┌─────────────────────────────┼─────────────────────────────────┐   │
│  │  ┌──────────────────────────┴──────────────────────────────┐  │   │
│  │  │  AppDbContext (EF Core)                                  │  │   │
│  │  │  - Users, Bookings, ServiceCategories, Notifications    │  │   │
│  │  │  - OnModelCreating (fluent config)                      │  │   │
│  │  └─────────────────────────────────────────────────────────┘  │   │
│  │  ┌──────────────────────────────────────────────────────────┐  │   │
│  │  │  Repository<T> (generic, thin wrapper)                    │  │   │
│  │  │  - GetById, GetAll, Add, Update, Delete                  │  │   │
│  │  └──────────────────────────────────────────────────────────┘  │   │
│  └─────────────────────────────────────────────────────────────────┘   │
├───────────────────────────────────────────────────────────────────────┤
│                    INFRASTRUCTURE LAYER                                │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐    │
│  │  PostgreSQL       │  │  ASP.NET Session │  │  Docker          │    │
│  │  (via Npgsql)     │  │  (phone auth)    │  │  Container       │    │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘    │
└───────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Implementation Detail |
|-----------|----------------|----------------------|
| **Razor Views** | Render HTML, handle form input, display booking status | Bootstrap 5 responsive, no JavaScript framework needed; minimal vanilla JS for UX enhancements |
| **Controllers** | Route HTTP requests, model binding, return View/Redirect | Thin -- delegate all logic to services. One controller per user-facing domain (Booking, Admin, Auth) |
| **ViewModels/DTOs** | Shape data for views, validate form input | DataAnnotations for validation. Separate from EF entities to avoid over-posting |
| **BookingService** | Booking lifecycle: create, lookup by phone, status changes | Orchestrates validation, persistence, and notification creation |
| **AdminService** | Dashboard queries, accept/decline actions, scheduling | Read-only queries for dashboard; write operations that change booking state |
| **NotificationService** | Create and retrieve status-update messages | Creates a Notification entity when booking status changes. Read by user-side status page |
| **SessionService** | Phone+name auth, session management | Uses ASP.NET Core `HttpContext.Session`. Stores user ID/phone. No Identity framework needed |
| **AppDbContext** | EF Core database context, migrations, entity config | Fluent API in OnModelCreating for relationships and indexes |
| **Repository\<T\>** | Generic data access wrapper | Thin wrapper over DbSet. Avoids scattering EF queries through services |
| **PostgreSQL** | Persistent storage | Hosted separately. EF Core migrations manage schema |

## Recommended Project Structure

```
ServiceCity/
├── ServiceCity.Web/                      # ASP.NET Core MVC project (startup project)
│   ├── Controllers/
│   │   ├── BookingController.cs          # User-facing: create, lookup, status
│   │   ├── AdminController.cs            # Admin dashboard, accept/decline
│   │   └── AuthController.cs             # Sign-in, sign-out (phone-based)
│   ├── Views/
│   │   ├── Booking/
│   │   │   ├── Create.cshtml             # Booking form (category, name, phone)
│   │   │   ├── Status.cshtml             # Booking status page with notifications
│   │   │   └── Lookup.cshtml             # Phone number lookup form
│   │   ├── Admin/
│   │   │   ├── Dashboard.cshtml          # All bookings, sorted by status
│   │   │   └── BookingDetail.cshtml      # Single booking detail + actions
│   │   ├── Auth/
│   │   │   └── SignIn.cshtml             # Name + phone form
│   │   └── Shared/
│   │       ├── _Layout.cshtml            # Main layout (navbar, footer)
│   │       └── _StatusBadge.cshtml       # Reusable status badge partial
│   ├── Models/                           # ViewModels only (not EF entities)
│   │   ├── BookingCreateViewModel.cs
│   │   ├── BookingStatusViewModel.cs
│   │   ├── AdminDashboardViewModel.cs
│   │   └── SignInViewModel.cs
│   ├── wwwroot/                          # Static assets
│   │   ├── css/                          # Bootstrap + custom CSS
│   │   └── js/                           # Minimal vanilla JS
│   ├── Program.cs                        # App configuration, DI registration
│   └── appsettings.json                  # Connection strings, config
│
├── ServiceCity.Core/                     # Domain/business logic (Class Library)
│   ├── Entities/                         # EF Core entity classes
│   │   ├── User.cs
│   │   ├── Booking.cs
│   │   ├── ServiceCategory.cs
│   │   ├── Notification.cs
│   │   └── BookingStatus.cs              # Enum: Pending, Accepted, Declined, Completed
│   ├── Services/                         # Business logic
│   │   ├── IBookingService.cs
│   │   ├── BookingService.cs
│   │   ├── IAdminService.cs
│   │   ├── AdminService.cs
│   │   ├── INotificationService.cs
│   │   ├── NotificationService.cs
│   │   ├── ISessionService.cs
│   │   └── SessionService.cs
│   └── Interfaces/                       # Repository interfaces
│       └── IRepository.cs
│
├── ServiceCity.Data/                     # Data access (Class Library)
│   ├── AppDbContext.cs                   # EF Core DbContext
│   ├── Repository.cs                     # Generic repository implementation
│   ├── Migrations/                       # EF Core migrations (auto-generated)
│   └── Configurations/                   # Fluent API entity configs
│       ├── UserConfiguration.cs
│       ├── BookingConfiguration.cs
│       └── NotificationConfiguration.cs
│
└── ServiceCity.sln                       # Solution file
```

### Structure Rationale

- **Web project (`ServiceCity.Web`):** Contains only MVC concerns -- controllers, views, ViewModels, static assets. References Core and Data projects. This is the composition root where DI is wired up.
- **Core project (`ServiceCity.Core`):** All business logic. No dependency on ASP.NET or EF directly (services depend on repository interfaces, not DbContext). This keeps the domain logic testable in isolation.
- **Data project (`ServiceCity.Data`):** EF Core concerns only -- DbContext, migrations, entity configurations, repository implementation. Depends on Core (for entity types and interfaces).
- **Entities in Core, not Data:** Entities are domain objects. Placing them in Core means Data depends on Core, never the reverse. This prevents EF concerns from leaking into business logic.
- **ViewModels in Web, not Core:** ViewModels are presentation-layer concerns. They shape data for specific views. Entities should never be passed directly to views (prevents over-posting and tight coupling).

## Architectural Patterns

### Pattern 1: Service Layer (Facade over Business Logic)

**What:** Controllers call service classes. Services orchestrate business operations -- validation, entity manipulation, cross-cutting concerns like notifications. Services depend on repository interfaces, not on EF Core directly.

**When to use:** Every non-trivial operation. Keeps controllers thin and testable.

**Trade-offs:**
- Pro: Business logic centralized, testable without HTTP context, swappable implementations
- Con: Extra abstraction layer. For CRUD-only endpoints it feels like boilerplate. Accept this for consistency.

**Example:**
```csharp
// In ServiceCity.Core/Services/BookingService.cs
public class BookingService : IBookingService
{
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IRepository<User> _userRepo;
    private readonly INotificationService _notificationService;

    public async Task<Booking> CreateBookingAsync(BookingCreateDto dto)
    {
        // 1. Find or create user by phone number
        var user = await _userRepo.FindAsync(u => u.Phone == dto.Phone)
                   ?? await _userRepo.AddAsync(new User { Name = dto.Name, Phone = dto.Phone });

        // 2. Create booking
        var booking = new Booking
        {
            UserId = user.Id,
            ServiceCategoryId = dto.CategoryId,
            Address = dto.Address,
            Description = dto.Description,
            Status = BookingStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _bookingRepo.AddAsync(booking);

        // 3. Create notification
        await _notificationService.CreateAsync(booking.Id,
            "Booking received. Admin will review your request shortly.");

        return booking;
    }
}
```

### Pattern 2: Generic Repository (Thin EF Core Wrapper)

**What:** A generic `IRepository<T>` / `Repository<T>` that wraps DbSet operations. Services depend on the interface, not on DbContext directly.

**When to use:** For this project scope, a generic repository keeps data access consistent without over-engineering. Avoid creating entity-specific repositories (IBookingRepository) until a specific entity needs custom queries that don't fit the generic pattern.

**Trade-offs:**
- Pro: Single implementation, testable via mocking IRepository<T>, consistent data access pattern
- Con: Generic repo can't express complex queries well. Add specific methods or use a query object pattern if needed later.

**Example:**
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    public async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();
    public async Task<T> AddAsync(T entity) { await _dbSet.AddAsync(entity); return entity; }
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
```

### Pattern 3: Status State Machine (Booking Lifecycle)

**What:** Booking status transitions are explicit and validated. Not every status can transition to every other status. The BookingService enforces valid transitions.

**When to use:** The core of the booking system. Prevents invalid states (e.g., "accepted" directly to "pending").

**Valid transitions:**
```
Pending ──→ Accepted ──→ Completed
   │            │
   └──→ Declined
```

**Example:**
```csharp
public enum BookingStatus
{
    Pending = 1,
    Accepted = 2,
    Declined = 3,
    Completed = 4
}

// In BookingService
private static readonly Dictionary<BookingStatus, BookingStatus[]> AllowedTransitions = new()
{
    [BookingStatus.Pending]  = new[] { BookingStatus.Accepted, BookingStatus.Declined },
    [BookingStatus.Accepted] = new[] { BookingStatus.Completed },
    [BookingStatus.Declined] = Array.Empty<BookingStatus>(),
    [BookingStatus.Completed] = Array.Empty<BookingStatus>()
};

public void TransitionStatus(Booking booking, BookingStatus newStatus)
{
    if (!AllowedTransitions[booking.Status].Contains(newStatus))
        throw new InvalidOperationException(
            $"Cannot transition from {booking.Status} to {newStatus}");

    booking.Status = newStatus;
    booking.UpdatedAt = DateTimeOffset.UtcNow;
    _bookingRepo.Update(booking);
}
```

## Data Flow

### User Booking Flow (Primary Read Path)

```
User opens site
    ↓
GET /Booking/Create
    → BookingController.Create()
    → Load ServiceCategories from DB
    → Return View with category dropdown
    ↓
User fills form (category, name, phone, address, description)
    ↓
POST /Booking/Create
    → BookingController.Create(BookingCreateViewModel)
    → Model binding + validation
    → BookingService.CreateBookingAsync(dto)
        → Find or create User by phone
        → Create Booking entity (Status = Pending)
        → NotificationService.CreateAsync() for confirmation message
        → SaveChangesAsync()
    → Redirect to /Booking/Status/{bookingId}
    ↓
GET /Booking/Status/{id}
    → BookingController.Status(id)
    → BookingService.GetBookingWithNotifications(id)
    → Return View with booking + notification history
```

### Admin Management Flow (Primary Write Path)

```
Admin opens dashboard
    ↓
GET /Admin/Dashboard
    → AdminController.Dashboard()
    → AdminService.GetBookingsGroupedByStatus()
    → Returns ViewModel with Pending, Accepted, Declined lists
    ↓
Admin clicks "Accept" on a pending booking
    ↓
POST /Admin/Accept
    → AdminController.Accept(bookingId, estimatedTime)
    → AdminService.AcceptBookingAsync(bookingId, estimatedTime)
        → Load booking, validate status is Pending
        → BookingService.TransitionStatus(booking, Accepted)
        → Set EstimatedArrivalTime
        → NotificationService.CreateAsync("Your booking is accepted. Technician arrives at {time}")
        → SaveChangesAsync()
    → Redirect to Dashboard (updated)
    ↓
Admin clicks "Decline" on a pending booking
    ↓
POST /Admin/Decline
    → AdminController.Decline(bookingId, reason)
    → AdminService.DeclineBookingAsync(bookingId, reason)
        → Load booking, validate status is Pending
        → BookingService.TransitionStatus(booking, Declined)
        → Set DeclineReason
        → NotificationService.CreateAsync($"Booking declined: {reason}")
        → SaveChangesAsync()
    → Redirect to Dashboard (updated)
```

### Booking Lookup Flow (Unauthenticated Read Path)

```
User visits site, wants to check existing booking
    ↓
GET /Booking/Lookup
    → BookingController.Lookup()
    → Return View with phone number input
    ↓
POST /Booking/Lookup
    → BookingController.Lookup(phoneNumber)
    → BookingService.FindBookingsByPhone(phoneNumber)
    → If one booking found: redirect to /Booking/Status/{id}
    → If multiple: show list
    → If none: show "No bookings found" message
```

### Notification Flow (Cross-Cutting)

```
Booking status changes (accept, decline)
    ↓
Service calls NotificationService.CreateAsync(bookingId, message)
    ↓
Notification entity saved to DB
    ↓
User visits /Booking/Status/{id}
    ↓
Controller loads booking with .Include(b => b.Notifications)
    ↓
View renders notification timeline (newest first)
```

### State Management

ServiceCity uses **server-side state only** in v1. No client-side state management library needed.

| State Type | Where It Lives | How It's Managed |
|------------|---------------|------------------|
| User session (auth) | ASP.NET Core Session (server-side cookie) | SessionService reads/writes HttpContext.Session |
| Booking data | PostgreSQL | EF Core reads/writes via services and repositories |
| Form state | HTML forms (POST-REDIRECT-GET) | Standard MVC pattern, no JS state |
| UI state (filters, sort) | Query string parameters on GET requests | Standard Razor pattern, no AJAX needed |

**No JavaScript framework.** Bootstrap 5 provides responsive layout and components. A small amount of vanilla JS handles UX enhancements (e.g., form validation feedback, date/time picker). No SPA or client-side state management complexity.

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 0-100 bookings/day | Current architecture (monolith, N-layer, single DB). No changes needed. |
| 100-1000 bookings/day | Add output caching for dashboard queries. Add database indexes on Booking.Status, Booking.CreatedAt, User.Phone. Consider read-only DbContext for dashboard. |
| 1000+/day or multi-shop | Split admin dashboard to separate read models (CQRS-lite with materialized views). Introduce background job processing for notifications. Consider extracting notification delivery to a queue. |

### Scaling Priorities

1. **First bottleneck:** Admin dashboard queries. Loading all bookings with includes on every page load. Fix with: pagination (always implement from day 1), output caching, and database indexes.
2. **Second bottleneck:** Notification creation inline during booking status changes (synchronous write). Fix with: background job queue if volume grows.

**v1 note:** None of these are concerns at v1 scale (single shop, manual management). The current architecture handles this comfortably.

## Anti-Patterns

### Anti-Pattern 1: Fat Controllers

**What people do:** Put business logic, database queries, and validation directly in controller actions.

**Why it's wrong:** Controllers become untestable monoliths. Logic can't be reused. Changing business rules requires touching web-layer code.

**Do this instead:** Controllers handle HTTP concerns only (model binding, returning views/redirects). All business logic lives in services. If a controller action is more than 5-10 lines, extract to a service method.

### Anti-Pattern 2: Entities as ViewModels

**What people do:** Pass EF Core entity objects directly to Razor views.

**Why it's wrong:** Over-posting vulnerability (malicious form fields can modify properties not intended for editing). View requirements leak into domain model design. Lazy loading exceptions when rendering views.

**Do this instead:** Create dedicated ViewModel/DTO classes for each view. Map from entities to ViewModels in the service layer or controller. Use only ViewModels in views.

### Anti-Pattern 3: Inline Notification Logic

**What people do:** Create notification records directly in BookingService or AdminService by instantiating DbContext.

**Why it's wrong:** Notification creation scatters across services. Changing notification behavior requires touching multiple classes. Testing booking logic now requires notification setup.

**Do this instead:** Centralize notification creation in NotificationService. Other services depend on INotificationService. This makes notification delivery swappable (e.g., push notifications later) without touching booking logic.

### Anti-Pattern 4: No Status Validation

**What people do:** Set `booking.Status = newStatus` anywhere without checking if the transition is valid.

**Why it's wrong:** Bookings can enter invalid states (e.g., "completed" going back to "pending"). Data integrity breaks silently.

**Do this instead:** Centralize status transitions in BookingService. Validate against an allowed-transitions map. Throw on invalid transitions. This is the core integrity rule of the system.

### Anti-Pattern 5: Premature Admin Auth Complexity

**What people do:** Build a full role-based access control system for a single-admin shop before it's needed.

**Why it's wrong:** Over-engineering. The admin is hardcoded (one shop, one admin). A simple admin password or PIN is sufficient for v1.

**Do this instead:** Use a simple admin password stored in appsettings or environment variable. Admin signs in with phone + admin PIN. Add proper RBAC when multi-admin or multi-shop becomes a requirement.

## Integration Points

### External Services

| Service | Integration Pattern | Notes |
|---------|---------------------|-------|
| PostgreSQL | Npgsql connection string via EF Core | Configured in appsettings.json. Docker compose for dev. Connection pooling handled by Npgsql |
| ASP.NET Session | In-memory (dev) / Redis (prod) | Session stores user auth state. In-memory is fine for single-container deploy. Add Redis if scaling to multiple instances |

### Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Web → Core | Direct method calls | Controllers inject service interfaces. DI wired in Program.cs |
| Core → Data | Via IRepository<T> interface | Services depend on interface, not DbContext. Keeps Core testable |
| Services → Services | Direct method calls via DI | NotificationService called by BookingService and AdminService |
| Data → PostgreSQL | EF Core + Npgsql | Single DbContext. Migrations manage schema |

### DI Registration (Program.cs)

```csharp
// Data layer
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Core layer
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

## Build Order Implications

The architecture supports this build sequence. Dependencies flow downward -- each phase builds on the previous:

```
Phase 1: Project Scaffold + Database
    │  (Solution, projects, EF Core setup, entities, initial migration)
    ↓
Phase 2: Auth (Session)
    │  (SessionService, SignIn page, phone+name session. Needed before booking)
    ↓
Phase 3: User Booking
    │  (BookingService, Create page, Status page, Lookup page.
    │   Depends on: Auth, DB)
    ↓
Phase 4: Admin Dashboard
    │  (AdminService, Dashboard page, BookingDetail page.
    │   Depends on: User Booking creating data to display)
    ↓
Phase 5: Admin Actions + Notifications
    │  (Accept/Decline, NotificationService, status transitions.
    │   Depends on: Admin Dashboard for UI, BookingService for state machine)
    ↓
Phase 6: Polish
       (Responsive QA, validation UX, empty states, error pages.
        Depends on: All features working)
```

**Rationale for this order:**
- Database schema must exist before anything reads/writes (Phase 1)
- Auth must exist before booking creation -- user identity anchors bookings (Phase 2)
- Booking creation must exist before admin can manage bookings (Phase 3 before 4)
- Admin actions create notification data, so NotificationService is bundled with admin actions (Phase 5)
- UX polish after features work -- don't polish forms that might change (Phase 6 last)

## Sources

- ASP.NET Core MVC documentation patterns (training knowledge, .NET 8-10 era)
- Microsoft eShopOnWeb reference application architecture (training knowledge)
- Entity Framework Core documentation, N-layer and Clean Architecture patterns (training knowledge)
- Service booking system domain patterns derived from common reservation/platform architectures (training knowledge)

---
*Architecture research for: ServiceCity AC service booking platform*
*Researched: 2026-06-16*
*Confidence: MEDIUM -- no external verification was possible in this environment. All claims are training-data based and should be validated against current ASP.NET Core 10 documentation when web access is available.*
