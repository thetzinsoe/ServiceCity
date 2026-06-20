using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Data;
using ServiceCity.Models;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;
using PhoneNumbers;
using System.Security.Cryptography;
using System.Text;

namespace ServiceCity.Controllers;

public class BookingController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int serviceCategoryId)
    {
        var category = await db.ServiceCategories.FindAsync(serviceCategoryId);
        if (category == null) return NotFound();

        var model = new BookingViewModel
        {
            ServiceCategoryId = serviceCategoryId
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("BookingSubmission")]
    public async Task<IActionResult> Create(BookingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (isValid, normalized, error) = ValidateAndNormalizePhone(model.CustomerPhone);
        if (!isValid)
        {
            ModelState.AddModelError("CustomerPhone", error!);
            return View(model);
        }

        var idempotencyKey = GenerateIdempotencyKey(normalized!, model.ServiceCategoryId, model.PreferredDate, model.CustomerName, model.Address);

        var existingBooking = await db.Bookings.FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey);
        if (existingBooking != null)
        {
            return RedirectToAction("Confirmation", new { referenceNumber = existingBooking.ReferenceNumber });
        }

        var referenceNumber = GenerateReferenceNumber();

        var booking = new Booking
        {
            ReferenceNumber = referenceNumber,
            ServiceCategoryId = model.ServiceCategoryId,
            CustomerName = model.CustomerName,
            CustomerPhone = model.CustomerPhone,
            CustomerPhoneNormalized = normalized,
            Address = model.Address,
            Description = model.Description,
            PreferredDate = DateTime.SpecifyKind(model.PreferredDate, DateTimeKind.Utc),
            PreferredTimeSlot = model.PreferredTimeSlot,
            Status = BookingStatus.Pending,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow
        };

        db.Bookings.Add(booking);
        db.Notifications.Add(new Notification
        {
            Booking = booking,
            Message = "Booking created — your request has been received.",
            StatusTo = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return RedirectToAction("Confirmation", new { referenceNumber });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string referenceNumber)
    {
        var booking = await db.Bookings
            .Include(b => b.ServiceCategory)
            .FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber);

        if (booking == null) return NotFound();

        return View(booking);
    }

    [HttpGet]
    public IActionResult Lookup()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lookup(string phoneNumber)
    {
        var (isValid, normalized, error) = ValidateAndNormalizePhone(phoneNumber);
        if (!isValid)
        {
            ModelState.AddModelError("phoneNumber", error!);
            return View();
        }

        var bookings = await db.Bookings
            .Include(b => b.ServiceCategory)
            .Where(b => b.CustomerPhoneNormalized == normalized)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View("LookupResults", bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Status(string referenceNumber)
    {
        var booking = await db.Bookings
            .Include(b => b.ServiceCategory)
            .FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber);

        if (booking == null) return NotFound();

        return View(booking);
    }

    private static string GenerateReferenceNumber()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var suffix = new string(Enumerable.Range(0, 8)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
        return $"SC-{suffix}";
    }

    private static string GenerateIdempotencyKey(string phone, int categoryId, DateTime date, string name, string address)
    {
        var data = $"{phone}-{categoryId}-{date:yyyyMMdd}-{name}-{address}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(bytes);
    }

    private static (bool IsValid, string? Normalized, string? Error) ValidateAndNormalizePhone(string input)
    {
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            var number = util.Parse(input, "MM");
            if (!util.IsValidNumber(number))
            {
                return (false, null, "Please enter a valid Myanmar phone number.");
            }
            var normalized = util.Format(number, PhoneNumberFormat.E164);
            return (true, normalized, null);
        }
        catch (NumberParseException)
        {
            return (false, null, "Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789).");
        }
    }
}
