---
status: testing
phase: 03-user-booking
source: [03-01-SUMMARY.md]
started: 2026-06-20T14:20:00Z
updated: 2026-06-20T14:20:00Z
---

## Current Test

number: 2
name: Fill and submit booking form
expected: |
  Fill in all required fields (name, phone like "09123456789", address, select tomorrow's date, pick a time slot). Click "Submit Booking". The form should submit and you should see a confirmation page with a reference number like "SC-XXXXXXXX".
awaiting: user response

## Tests

### 1. Select service category and open booking form
expected: From the home page, click on a service category (e.g., "Repair"). You should see a booking form with fields for Name, Phone, Address, Description, Preferred Date, and Time Slot.
result: issue
reported: "Server error: 'The entity type Booking requires a primary key to be defined'"
severity: blocker

### 2. Fill and submit booking form
expected: Fill in all required fields (name, phone like "09123456789", address, select tomorrow's date, pick a time slot). Click "Submit Booking". The form should submit and you should see a confirmation page with a reference number like "SC-XXXXXXXX".
result: pending

### 3. See booking confirmation
expected: The confirmation page shows "Booking Confirmed" heading, the reference number prominently in a green alert box, the service category name, preferred date, and a "Check Status" link.
result: pending

### 4. Navigate to lookup page
expected: Click "Check Status" in the navigation bar (or the link on the confirmation page). You should see a "Lookup Bookings" page with a phone number input and "Find Bookings" button.
result: pending

### 5. Look up bookings by phone
expected: Enter the phone number you used for booking (e.g., "09123456789"). Click "Find Bookings". You should see a list showing your booking with the reference number, service category, status, and date.
result: pending

### 6. View booking status page
expected: Click on a booking from the lookup results. You should see the booking status page showing the reference number, service category, customer name, address, preferred date, time slot, and current status (Pending).
result: pending

### 7. Duplicate submission prevention
expected: Try submitting the same booking form again with identical data. You should be redirected to the same confirmation page (same reference number), not create a duplicate booking.
result: pending

### 8. Cold Start Smoke Test
expected: Kill any running server/service. Clear ephemeral state. Start the application from scratch via docker compose up. Server boots without errors, migrations complete, and the home page loads with service categories.
result: pending

## Summary

total: 8
passed: 0
issues: 0
pending: 8
skipped: 0
blocked: 0

## Gaps

- truth: "Navigating to a service category page shows the booking form"
  status: failed
  reason: "Docker image cached old build without BookingConfiguration changes — 'The entity type Booking requires a primary key to be defined'"
  severity: blocker
  test: 1
  artifacts: [docker-compose.yml, ServiceCity/Dockerfile]
  missing: []
  resolution: "docker compose down && docker compose up --build -d — rebuilds image with all commits"
  resolved: true

