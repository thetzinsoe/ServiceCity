using ServiceCity.Core.Entities;

namespace ServiceCity.Data.Interfaces;

public interface IBookingRepository
{
    IQueryable<Booking> Query();
    Task<Booking?> FindAsync(int id);
    Task<Booking?> GetByReferenceAsync(string referenceNumber);
    Task<List<Booking>> GetByUserIdAsync(int userId);
    Task<Dictionary<Core.Enums.BookingStatus, int>> GetCountsByStatusAsync();
    Task<List<Booking>> SearchAsync(string term);
    void Add(Booking booking);
}
