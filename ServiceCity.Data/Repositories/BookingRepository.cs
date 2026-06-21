using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Data.Interfaces;

namespace ServiceCity.Data.Repositories;

public class BookingRepository(AppDbContext db) : IBookingRepository
{
    public IQueryable<Booking> Query() => db.Bookings.AsNoTracking();

    public async Task<Booking?> FindAsync(int id)
        => await db.Bookings.FindAsync(id);

    public async Task<Booking?> GetByReferenceAsync(string referenceNumber)
        => await db.Bookings
            .Include(b => b.ServiceCategory)
            .FirstOrDefaultAsync(b => b.ReferenceNumber == referenceNumber);

    public async Task<List<Booking>> GetByUserIdAsync(int userId)
        => await db.Bookings
            .Include(b => b.ServiceCategory)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<Dictionary<Core.Enums.BookingStatus, int>> GetCountsByStatusAsync()
        => await db.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count);

    public async Task<List<Booking>> SearchAsync(string term)
        => await db.Bookings
            .Include(b => b.ServiceCategory)
            .Where(b =>
                b.ReferenceNumber.Contains(term) ||
                b.CustomerPhone.Contains(term) ||
                (b.CustomerPhoneNormalized != null && b.CustomerPhoneNormalized.Contains(term)) ||
                b.CustomerName.Contains(term))
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public void Add(Booking booking) => db.Bookings.Add(booking);
}
