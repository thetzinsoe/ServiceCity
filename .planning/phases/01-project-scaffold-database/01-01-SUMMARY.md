---
phase: 01-project-scaffold-database
plan: 01
subsystem: data-model
tags: [entities, dbcontext, ef-core, migrations, seed-data]
requires: []
provides:
  - ServiceCity.Core (class library, net10.0)
  - ServiceCity.Data (class library, net10.0)
  - Initial EF Core migration with 4 tables + seed data
affects:
  - ServiceCity.slnx (solution structure)
  - ServiceCity/appsettings.json (connection string)
tech-stack:
  added:
    - Microsoft.EntityFrameworkCore 10.0.9
    - Npgsql.EntityFrameworkCore.PostgreSQL 10.0.2
    - Microsoft.EntityFrameworkCore.Design 10.0.9
    - dotnet-ef CLI 10.0.9
  patterns:
    - 3-project assembly structure (Web/Core/Data)
    - Fluent API entity configuration via IEntityTypeConfiguration<T>
    - Enum-to-int conversion via HasConversion<int>()
    - Seed data via HasData() with explicit IDs
    - Design-time DbContext factory via IDesignTimeDbContextFactory<T>
key-files:
  created:
    - ServiceCity.Core/ServiceCity.Core.csproj
    - ServiceCity.Core/Enums/BookingStatus.cs
    - ServiceCity.Core/Enums/PreferredTimeSlot.cs
    - ServiceCity.Core/Entities/User.cs
    - ServiceCity.Core/Entities/ServiceCategory.cs
    - ServiceCity.Core/Entities/Booking.cs
    - ServiceCity.Core/Entities/Notification.cs
    - ServiceCity.Data/ServiceCity.Data.csproj
    - ServiceCity.Data/AppDbContext.cs
    - ServiceCity.Data/DesignTimeDbContextFactory.cs
    - ServiceCity.Data/SeedData.cs
    - ServiceCity.Data/Configurations/UserConfiguration.cs
    - ServiceCity.Data/Configurations/ServiceCategoryConfiguration.cs
    - ServiceCity.Data/Configurations/BookingConfiguration.cs
    - ServiceCity.Data/Configurations/NotificationConfiguration.cs
    - ServiceCity.Data/Migrations/20260617181115_InitialCreate.cs
    - ServiceCity.Data/Migrations/20260617181115_InitialCreate.Designer.cs
    - ServiceCity.Data/Migrations/AppDbContextModelSnapshot.cs
  modified:
    - ServiceCity.slnx (added Core and Data project entries)
    - ServiceCity/appsettings.json (added ConnectionStrings.DefaultConnection)
decisions:
  - "ServiceCity.Core has no EF Core dependency — pure entities/enums, no IEntityTypeConfiguration coupling"
  - "Enum storage uses HasConversion<int>() mapping to int in DB, BookingStatus.Pending defaults to 0"
  - "PhoneNumberNormalized is nullable (v1), indexed but not unique (multi-user phase)"
  - "Booking FK relationships use DeleteBehavior.Restrict (no accidental cascade deletion of bookings)"
  - "IDesignTimeDbContextFactory in Data project avoids startup-project dependency for migration tooling"
metrics:
  duration: ""
  completed_date: "2026-06-17"
---

# Phase 01 Plan 01: Scaffold 3-Project Solution with Entity Definitions, DbContext, and Initial Migration

**One-liner:** Established the ServiceCity data model with 4 entities, 2 enums, EF Core DbContext with Fluent API configurations, and an initial PostgreSQL migration creating Users, ServiceCategories, Bookings, and Notifications tables with seed data.

## Tasks Executed

| # | Task | Commit | Status |
|---|------|--------|--------|
| 1 | Create Core and Data class library projects with entity definitions | 38d1f73 | Complete |
| 2 | Add EF Core compile-time packages, create AppDbContext with Fluent API configurations | fb702d3 | Complete |
| 3 | Add EF Core CLI package, create IDesignTimeDbContextFactory, initial migration with seed data | 066b733 | Complete |

## Verification Results

### Build

```
dotnet build ServiceCity.slnx
```
- ServiceCity.Core: Build succeeded, 0 warnings, 0 errors
- ServiceCity.Data: Build succeeded, 0 warnings, 0 errors
- ServiceCity (Web): Build succeeded, 0 warnings, 0 errors

### Migration SQL

```
dotnet ef migrations script --project ServiceCity.Data
```
- 4 CREATE TABLE statements: Users, ServiceCategories, Bookings, Notifications
- 4 INSERT statements for seed ServiceCategory data (Repair, Maintenance, Installation, Gas Refill)
- All foreign keys present with correct delete behaviors (RESTRICT for Bookings, CASCADE for Notifications)
- All indexes present: IX_Bookings_ReferenceNumber (unique), IX_Bookings_Status_CreatedAt, IX_Bookings_UserId_Status, IX_Notifications_BookingId_CreatedAt, IX_Users_PhoneNumber, IX_Users_PhoneNumberNormalized
- Enum columns stored as integer: Status, PreferredTimeSlot, StatusFrom, StatusTo
- Column constraints match plan: Name(200), PhoneNumber(20), ReferenceNumber(12 unique), Address(500), Description(2000/500), Message(1000)

### Must-Have Propositions Verified

| Proposition | Status |
|-------------|--------|
| dotnet build succeeds for ServiceCity.Core (class library, net10.0) | PASS |
| dotnet build succeeds for ServiceCity.Data (class library, net10.0, depends on Core) | PASS |
| EF Core initial migration creates tables: Users, Bookings, ServiceCategories, Notifications | PASS |
| Users table has PhoneNumberNormalized column (nullable) | PASS |
| Bookings table has ReferenceNumber column (string, max 12) | PASS |
| Bookings table has Status column (int, maps to BookingStatus enum) | PASS |
| Seed data inserts 4 ServiceCategory rows | PASS |

## Entity Summary

| Entity | Table | Key Columns | Relationships |
|--------|-------|-------------|---------------|
| User | Users | Id, Name(200), PhoneNumber(20), PhoneNumberNormalized(20) nullable, IsAdmin, CreatedAt | HasMany Bookings |
| ServiceCategory | ServiceCategories | Id, Name(100), Description(500), SortOrder | HasMany Bookings |
| Booking | Bookings | Id, ReferenceNumber(12 unique), UserId FK, ServiceCategoryId FK, Address(500), Description(2000), PreferredDate, PreferredTimeSlot(int), Status(int), CreatedAt, UpdatedAt | BelongsTo User, BelongsTo ServiceCategory, HasMany Notifications |
| Notification | Notifications | Id, BookingId FK (CASCADE), Message(1000), StatusFrom(int) nullable, StatusTo(int) nullable, IsViewed, CreatedAt | BelongsTo Booking |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] SeedData placeholder created in Task 2 to prevent build failure**
- **Found during:** Task 2
- **Issue:** AppDbContext.OnModelCreating calls `SeedData.Seed(modelBuilder)`, but the SeedData class is not created until Task 3. Without it, Task 2 build would fail.
- **Fix:** Created a minimal placeholder `SeedData.cs` in Task 2 with an empty `Seed()` method. The file was fully populated with actual seed entries in Task 3.
- **Files modified:** ServiceCity.Data/SeedData.cs (created as placeholder, replaced in Task 3)
- **Commit:** fb702d3 (placeholder), 066b733 (actual)

## Threat Surface Scan

All threat surfaces match the plan's `<threat_model>`. No new surfaces introduced.

- T-01-01 (Information Disclosure): Connection string in appsettings.json — accepted per plan, dev credentials only
- T-01-02 (Elevation of Privilege): IsAdmin boolean defined with no auth — accepted per plan, Auth in Phase 2
- T-01-03 (Tampering): ReferenceNumber uniqueness via DB index — accepted per plan, random generation in Phase 3

## Self-Check: PASSED

- [x] All created files exist on disk
- [x] All 3 commits (38d1f73, fb702d3, 066b733) confirmed in git log
- [x] dotnet build ServiceCity.slnx succeeds with 0 errors
- [x] Migration SQL creates 4 application tables + seed data
- [x] No untracked files left (except .claude/ which is managed by orchestrator)
