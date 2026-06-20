using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;
using ServiceCity.Data;

namespace ServiceCity.Controllers;

[Authorize]
public class AdminController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var bookings = await db.Bookings
            .Include(b => b.ServiceCategory)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var groupedBookings = bookings.GroupBy(b => b.Status)
            .ToDictionary(g => g.Key, g => g.ToList());

        return View(groupedBookings);
    }

    public async Task<IActionResult> Details(int id)
    {
        var booking = await db.Bookings
            .Include(b => b.ServiceCategory)
            .Include(b => b.Notifications)
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

        AddNotification(booking, $"Accepted - estimated arrival: {arrivalTime:g}");
        
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

        AddNotification(booking, $"Declined - reason: {reason}");

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
