---
phase: 01-project-scaffold-database
plan: 02
subsystem: web, database, infra
tags: [ef-core, npgsql, postgresql, docker, aspnet-core-mvc, razor, bootstrap-5]

requires:
  - phase: 01-01
    provides: "3-project solution (Web/Core/Data), entity models, AppDbContext with Fluent API configs, EF Core migration with seed data"
provides:
  - "Web project wired to Core and Data via project references and EF Core DI"
  - "Home page displaying 4 service categories from PostgreSQL in Bootstrap card grid"
  - "Multi-project Dockerfile with layer-cached restore (3 .csproj files)"
  - "Docker Compose orchestrating app + PostgreSQL with health check, auto-migration, seed data"
  - "Walking skeleton proving full stack: HTTP -> Razor -> EF Core -> PostgreSQL"
affects:
  - "02-auth-session"
  - "03-user-booking"
  - "04-admin-dashboard"
  - "05-admin-actions-notifications"
  - "06-polish"

tech-stack:
  added:
    - "Microsoft.EntityFrameworkCore 10.0.* (Web project)"
    - "Npgsql.EntityFrameworkCore.PostgreSQL 10.0.* (Web project)"
    - "Microsoft.EntityFrameworkCore.Design 10.0.* (Web project)"
  patterns:
    - "Primary constructor DI (C# 12+) for controller dependencies"
    - "Auto-migration on startup via IServiceScopeFactory + Database.Migrate()"
    - "Multi-stage Docker build with .csproj copy before restore for layer caching"
    - "Docker Compose health check gating (pg_isready) before app startup"

key-files:
  created:
    - "ServiceCity/Dockerfile"
    - "docker-compose.yml"
    - ".dockerignore"
  modified:
    - "ServiceCity/ServiceCity.csproj"
    - "ServiceCity/Program.cs"
    - "ServiceCity/Controllers/HomeController.cs"
    - "ServiceCity/Views/Home/Index.cshtml"
    - "ServiceCity/Views/_ViewImports.cshtml"

key-decisions:
  - "Used primary constructor (C# 12+) for HomeController — cleaner DI without field storage"
  - "Placed Migrate() after UseRouting() but before UseAuthorization() — ensures DB ready before any request middleware"
  - "Build context changed from ./ServiceCity to . (project root) — required for multi-project Dockerfile to access Core/Data dirs"
  - "Kept ErrorViewModel passthrough in Error action — Error.cshtml still references the model"

patterns-established:
  - "Controller DI: Primary constructor with AppDbContext, no private field needed"
  - "Startup migration: ServiceProvider scope + GetRequiredService<AppDbContext>().Database.Migrate()"
  - "Docker: COPY all .csproj first -> restore -> COPY source -> publish (layer caching)"
  - "Docker Compose: depends_on db service_healthy -> app starts only after pg_isready passes"

requirements-completed: []

duration: 22min
completed: 2026-06-17
---

# Phase 01 Plan 02: Database Wiring + Walking Skeleton Finalization Summary

**Web project wired to Core/Data, Home page displaying 4 service categories from PostgreSQL via EF Core, Docker Compose full stack with auto-migration — walking skeleton proves end-to-end viability.**

## Performance

- **Duration:** ~22 min
- **Started:** 2026-06-17T23:03:00Z
- **Completed:** 2026-06-17T23:25:26Z
- **Tasks:** 3
- **Files modified:** 5 modified, 3 created

## Accomplishments

- Web project compiles with Core and Data project references, zero build errors
- Program.cs registers AppDbContext with PostgreSQL (`UseNpgsql`) and auto-migrates on startup
- HomeController queries ServiceCategories ordered by SortOrder from the database
- Home page renders responsive Bootstrap 5 card grid with all 4 service categories
- Docker Compose builds and starts both containers (app + PostgreSQL 17), app connects to DB automatically
- Migrations apply automatically on container startup; seed data confirmed (4 categories)
- End-to-end verification: `curl http://localhost:5124/` returns ServiceCity with all categories

## Task Commits

Each task was committed atomically:

1. **Task 1: Wire Web project to Core/Data, register EF Core, update HomeController** - `6c3b0e5` (feat)
2. **Task 2: Update Home page to display service categories** - `ae15f4c` (feat)
3. **Task 3: Update Dockerfile for multi-project structure and verify Docker Compose end-to-end** - `778bfb7` (feat)

## Files Created/Modified

- `ServiceCity/ServiceCity.csproj` - Added EF Core, Npgsql, Design packages + Core/Data project references
- `ServiceCity/Program.cs` - Registered AppDbContext with Npgsql, added auto-migration on startup
- `ServiceCity/Controllers/HomeController.cs` - Primary constructor DI, queries ServiceCategories from DB
- `ServiceCity/Views/Home/Index.cshtml` - Replaced placeholder with Bootstrap card grid showing categories from DB
- `ServiceCity/Views/_ViewImports.cshtml` - Added Core.Entities and Core.Enums usings
- `ServiceCity/Dockerfile` - Multi-stage build with 3-project .csproj layer caching
- `docker-compose.yml` - Updated build context to `.`, Dockerfile path to `ServiceCity/Dockerfile`
- `.dockerignore` - Excludes bin/obj/.git but includes Core/Data source dirs

## Decisions Made

None beyond plan specification. The plan was precise and all architectural decisions were pre-determined.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] ErrorViewModel missing from Error action broke Error.cshtml**

- **Found during:** Task 1 (HomeController update)
- **Issue:** Plan's HomeController example removed `using ServiceCity.Models;` and `using System.Diagnostics;` and the Error method became `return View()` without passing `ErrorViewModel`. However, `Error.cshtml` starts with `@model ErrorViewModel` and accesses `Model.ShowRequestId` and `Model.RequestId` — passing `null` model would crash.
- **Fix:** Restored `using ServiceCity.Models;` and `using System.Diagnostics;`, kept the original Error method body: `return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });`
- **Files modified:** `ServiceCity/Controllers/HomeController.cs`
- **Verification:** Build succeeds, ErrorViewModel namespace resolves correctly
- **Committed in:** `6c3b0e5` (Task 1 commit)

**2. [Rule 3 - Blocking] Dockerfile, docker-compose.yml, and .dockerignore not present in worktree**

- **Found during:** Task 3 (Docker setup)
- **Issue:** These files existed only in the main repo's working directory, not tracked in the worktree base commit `c240e64`. Dockerfile never existed anywhere — needed to be created from scratch.
- **Fix:** Created `ServiceCity/Dockerfile` from plan spec, copied `docker-compose.yml` and `.dockerignore` from main repo, then updated docker-compose.yml per plan (context: `.`, dockerfile: `ServiceCity/Dockerfile`).
- **Files modified:** `ServiceCity/Dockerfile` (created), `docker-compose.yml` (created/updated), `.dockerignore` (copied)
- **Verification:** `docker compose build` succeeds, all 3 files present and correct
- **Committed in:** `778bfb7` (Task 3 commit)

**3. [Rule 3 - Blocking] Stale containers blocked `docker compose up`**

- **Found during:** Task 3 (Docker Compose verification)
- **Issue:** Containers `servicecity-db` and `servicecity-app` from a prior run still existed, causing "Conflict. The container name is already in use" error.
- **Fix:** Ran `docker rm -f servicecity-db servicecity-app` before retrying `docker compose up -d`.
- **Files modified:** None (runtime operation)
- **Verification:** `docker compose up -d` succeeded on retry, both containers healthy
- **Committed in:** `778bfb7` (Task 3 commit)

---

**Total deviations:** 3 auto-fixed (1 bug, 2 blocking)
**Impact on plan:** All auto-fixes necessary for correctness and task completion. No scope creep.

## Issues Encountered

- `libgssapi_krb5.so.2` warning in Docker container logs — Npgsql looks for GSSAPI/Kerberos library for advanced auth. Non-critical for dev (uses password auth). Cosmetic only.
- dotnet SDK only available via Windows path `/mnt/c/Program Files/dotnet/dotnet.exe` — WSL Linux SDK not installed. Used Windows dotnet for build/publish.

## User Setup Required

None — no external service configuration required.

## Next Phase Readiness

- Walking skeleton fully operational — any developer can run `docker compose up` and see a working app with database
- Home page renders 4 service categories from PostgreSQL via EF Core
- Core and Data projects accessible from Web via project references — ready for feature development
- Auto-migration ensures schema stays in sync with code on every startup
- Phase 02 (Auth) can build on this foundation — AppDbContext is registered and available

## Self-Check: PASSED

All files verified present. All 3 task commits confirmed in git history.

---
*Phase: 01-project-scaffold-database*
*Completed: 2026-06-17*
