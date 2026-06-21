using ServiceCity.Core.Entities;

namespace ServiceCity.Data.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);
    Task<List<Notification>> GetByBookingIdAsync(int bookingId);
}
