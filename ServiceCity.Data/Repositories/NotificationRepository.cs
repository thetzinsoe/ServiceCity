using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Data.Interfaces;

namespace ServiceCity.Data.Repositories;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public void Add(Notification notification) => db.Notifications.Add(notification);

    public async Task<List<Notification>> GetByBookingIdAsync(int bookingId) =>
        await db.Notifications
            .Where(n => n.BookingId == bookingId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
}
