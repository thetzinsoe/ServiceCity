---
name: servicecity-dev
description: Quick development tasks for the ServiceCity ASP.NET Core MVC project. Use for adding features, fixing bugs, updating views, and committing changes — following the project's conventions and patterns.
---

# ServiceCity Dev Skill

Guidelines for working on the ServiceCity air conditioning service booking platform.

## Project Context

- **Stack:** ASP.NET Core MVC (.NET 10), EF Core 10, PostgreSQL 16+, Bootstrap 5.3
- **Auth:** Phone number + name session-based (no email/password in v1)
- **Pattern:** Service layer for business logic, EF Core for data, Razor views + Bootstrap
- **Target:** Mobile-first responsive web for Myanmar users

## Key Conventions

### Controllers
- Thin controllers that delegate to service layer
- Use `[ValidateAntiForgeryToken]` on POST actions
- Return `IActionResult` with appropriate status codes

### Views
- Razor `.cshtml` with Bootstrap 5 classes
- Mobile-first: test at 375px viewport width
- Use `_Layout.cshtml` for consistent chrome
- Use `_ValidationScripts` partial for client-side validation
- Page headers follow pattern: title + count badge + search bar

### Data Layer
- Entities in `ServiceCity.Core/Entities/`
- Configurations in `ServiceCity.Data/Configurations/` (use `IEntityTypeConfiguration<T>`)
- Migrations managed via `dotnet ef migrations add` / `dotnet ef database update`
- Connection string from environment (`ConnectionStrings__DefaultConnection`)

### Security
- `.env` file for local secrets — NEVER commit (in `.gitignore`)
- Passwords hashed with BCrypt
- `docker-compose.yml` uses `${VAR}` references, not hardcoded values
- `appsettings.json` has dev-only localhost connection string

### Git Workflow
- `main` branch only for this single-developer project
- Commit often with descriptive messages
- Push after each meaningful change set

## Common Tasks

### Adding a new page
1. Add action method in the appropriate controller
2. Create Razor view in `Views/<Controller>/`
3. Add any needed DTOs in `ServiceCity.Services/DTOs/`
4. Add service method in `Impl/<Service>.cs` and interface in `Interfaces/IServices.cs`

### Updating the database schema
1. Update entity class in `ServiceCity.Core/Entities/`
2. Update configuration in `ServiceCity.Data/Configurations/`
3. Run: `dotnet ef migrations add <MigrationName> --project ServiceCity.Data`
4. Run: `dotnet ef database update --project ServiceCity.Data`

### Testing with Docker
```bash
docker compose up --build
# App: http://localhost:5124
# Setup admin: http://localhost:5124/Auth/Setup
```
