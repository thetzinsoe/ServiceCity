using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;
using ServiceCity.Data;
using ServiceCity.Models;
using System.Security.Claims;

namespace ServiceCity.Controllers;

[Authorize]
public class AdminController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Dashboard(string? search, string? status)
    {
        var query = db.Bookings
            .Include(b => b.ServiceCategory)
            .AsQueryable();

        // Status filter
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsed))
        {
            query = query.Where(b => b.Status == parsed);
        }

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.ReferenceNumber.Contains(term) ||
                b.CustomerPhone.Contains(term) ||
                b.CustomerPhoneNormalized.Contains(term) ||
                b.CustomerName.Contains(term));
        }

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            Search = search,
            StatusFilter = status,
            GroupedBookings = bookings.GroupBy(b => b.Status)
                .ToDictionary(g => g.Key, g => g.ToList()),
            Counts = bookings.GroupBy(b => b.Status)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var booking = await db.Bookings
            .Include(b => b.ServiceCategory)
            .Include(b => b.Notifications.OrderByDescending(n => n.CreatedAt))
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id, DateTime arrivalTime)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.Pending) return NotFound();

        booking.Status = BookingStatus.Accepted;
        booking.EstimatedArrivalTime = DateTime.SpecifyKind(arrivalTime, DateTimeKind.Utc);
        booking.UpdatedAt = DateTime.UtcNow;

        AddNotification(booking, "Booking accepted — technician will arrive at the estimated time.");

        await db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(int id, string reason)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.Pending) return NotFound();

        booking.Status = BookingStatus.Declined;
        booking.DeclineReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;

        AddNotification(booking, $"Booking declined — {reason}");

        await db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InProgress(int id)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.Accepted) return NotFound();

        booking.Status = BookingStatus.InProgress;
        booking.UpdatedAt = DateTime.UtcNow;

        AddNotification(booking, "Service in progress — technician is on site.");

        await db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var booking = await db.Bookings.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.InProgress) return NotFound();

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;

        AddNotification(booking, "Service completed. Thank you for choosing ServiceCity!");

        await db.SaveChangesAsync();
        return RedirectToAction("Details", new { id });
    }

    public async Task<IActionResult> Customers(string? search)
    {
        var query = db.Users.Where(u => !u.IsAdmin).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Name.Contains(term) ||
                u.PhoneNumber.Contains(term) ||
                (u.PhoneNumberNormalized != null && u.PhoneNumberNormalized.Contains(term)));
        }

        var customers = await query
            .Select(u => new CustomerViewModel
            {
                Id = u.Id,
                Name = u.Name,
                PhoneNumber = u.PhoneNumber,
                BookingCount = db.Bookings.Count(b => b.UserId == u.Id),
                LastBooking = db.Bookings
                    .Where(b => b.UserId == u.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => (DateTime?)b.CreatedAt)
                    .FirstOrDefault()
            })
            .OrderByDescending(c => c.BookingCount)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return View(customers);
    }

    public async Task<IActionResult> CustomerDetail(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user == null || user.IsAdmin) return NotFound();

        var bookings = await db.Bookings
            .Include(b => b.ServiceCategory)
            .Where(b => b.UserId == id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        ViewBag.Customer = user;
        return View(bookings);
    }

    private void AddNotification(Booking booking, string message)
    {
        db.Notifications.Add(new Notification
        {
            BookingId = booking.Id,
            Message = message,
            StatusTo = booking.Status,
            CreatedAt = DateTime.UtcNow
        });
    }
}
