# Git Branch Workflow for ServiceCity

## Branch Naming Convention

```
phase-{NN}-{short-description}
```

Examples:
- `phase-10-notification-system`
- `phase-11-booking-edit-cancel`
- `phase-12-admin-analytics`

## Workflow for Each Phase

### 1. Start a new phase
```bash
# Create and switch to new branch
git checkout -b phase-10-notification-system

# Do your work...
# Commit frequently with descriptive messages
git add -A
git commit -m "feat(phase-10): add notification bell icon"
git commit -m "feat(phase-10): implement notification dropdown"
```

### 2. Finish a phase
```bash
# Push branch to GitHub
git push origin phase-10-notification-system

# Merge to main
git checkout main
git merge phase-10-notification-system --no-ff -m "Merge phase-10: notification system"

# Push main
git push origin main

# Optional: delete the branch
git branch -d phase-10-notification-system
```

### 3. Quick fixes (not phase-specific)
```bash
# Use gsd-quick workflow
/gsd-quick "fix description"
```

## Current Phase Status

| Phase | Description | Status |
|-------|-------------|--------|
| 01-06 | Initial development | ✅ Complete (on main) |
| 07 | Customer Registration | ✅ Complete (on main) |
| 08 | Booking Experience Polish | ✅ Complete (on main) |
| 09 | Layered Architecture | ✅ Complete (on main) |
| 10 | Next phase | 🔜 Create branch when starting |

## Benefits

- Each phase is isolated in its own branch
- Easy to review changes per phase
- Can revert a phase if needed
- Clean git history on main
