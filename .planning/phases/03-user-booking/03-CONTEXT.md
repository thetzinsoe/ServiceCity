---
phase: 03-user-booking
created: 2026-06-20T00:00:00Z
status: ready
---

# Phase 03: User Booking — Context

## Phase Goal

A user can select a service category, fill out a booking form with name/phone/address/description, pick a preferred date/time, and receive a reference number. They can look up their booking by phone number and see its status.

## Success Criteria

1. User navigates to the booking form, selects a category from the dropdown, fills in all fields, selects a preferred date/time slot, and submits — receives a confirmation page with a unique SC-XXXXXXXX reference number
2. User enters their phone number on the lookup page and sees a list of all their bookings (or "No bookings found" if none exist)
3. User clicks a booking from the lookup results and sees the booking status page (status = Pending) with the booking details
4. Rapid double-clicking the submit button creates only one booking (duplicate submission prevention)
5. More than 5 booking submissions from the same phone number within 1 hour are rejected with a rate limit message

## Requirements

- BOOK-01: Booking form with service category selection
- BOOK-02: Customer info fields (name, phone, address, description)
- BOOK-03: Preferred date/time selection
- BOOK-04: Unique reference number generation (SC-XXXXXXXX)
- BOOK-05: Phone number lookup for booking status
- BOOK-06: Booking status page display
- CROS-05: Phone number validation for booking

## Dependencies

- Phase 2 (Auth + phone normalization) — COMPLETE
- User entity with phone normalization from Phase 2
- libphonenumber-csharp already installed

## UI Requirements

- Booking creation form (mobile-first)
- Confirmation page with reference number
- Phone lookup page
- Booking status page with status badges

## Technical Considerations

- Reference numbers should be non-sequential (SC-XXXXXXXX) to prevent correlation
- Rate limiting on booking submissions (5 per phone per hour)
- Duplicate submission prevention (Idempotency key or DB constraint)
- Phone number normalization using existing libphonenumber integration
- Service categories already seeded in Phase 1

## Entities Needed

- Booking (new)
- ServiceCategory (exists from Phase 1)
- User (exists from Phase 2, phone normalized)

---

*Context created: 2026-06-20*
