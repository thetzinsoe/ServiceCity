---
status: testing
phase: 02-auth-session
source: [02-VERIFICATION.md]
started: 2026-06-19T18:35:00Z
updated: 2026-06-19T18:35:00Z
---

## Current Test

number: 1
name: Full admin sign-up and sign-in flow
expected: |
  Setup form creates admin row with hashed password. Sign-in issues auth cookie.
  Redirected to /Admin/Dashboard showing 'Admin Dashboard' heading. Nav shows Admin + Sign Out.
awaiting: user response

## Tests

### 1. Full admin sign-up and sign-in flow
expected: Setup form creates admin row with hashed password. Sign-in issues auth cookie. Redirected to /Admin/Dashboard showing 'Admin Dashboard' heading. Nav shows Admin + Sign Out.
result: pending

### 2. Route protection — unauthenticated /Admin/* redirects
expected: Navigate to /Admin/Dashboard while signed out → redirected to /Auth/SignIn
result: pending

### 3. Sign Out — cookie cleared and redirect to SignIn
expected: Click Sign Out → auth cookie cleared, browser redirected to /Auth/SignIn, nav shows "Sign In"
result: pending

### 4. Phone validation — Myanmar formats accepted, invalid rejected
expected: Navigate to /Auth/Setup, enter phone numbers in various formats (09-xxx, +959xxx). Valid formats stored as E.164. Invalid formats rejected with clear error.
result: pending

## Summary

total: 4
passed: 0
issues: 0
pending: 4
skipped: 0
blocked: 0

## Gaps
