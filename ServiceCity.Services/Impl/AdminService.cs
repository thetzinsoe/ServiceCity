using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;
using ServiceCity.Data;
using ServiceCity.Data.Interfaces;
using ServiceCity.Services.DTOs;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Services.Impl;

public class AdminService(
    AppDbContext db,
    IBookingRepository bookingRepo,
    IUserRepository userRepo,
    INotificationRepository notificationRepo) : IAdminService
{
    public async Task<AdminDashboardDto> GetDashboardAsync(string? search, string? status)
    {
        var bookings = await QueryBookings(search, status);
        return BuildDashboardDto(bookings, search, status);
    }

    public async Task<AdminDashboardDto> GetDrilldownAsync(string status, string? search)
    {
        if (!Enum.TryParse<BookingStatus>(status, true, out _))
            return new AdminDashboardDto { StatusFilter = status, Search = search };

        var bookings = await QueryBookings(search, status);
        return BuildDashboardDto(bookings, search, status);
    }

    public async Task<BookingDetailDto?> GetBookingDetailAsync(int id)
    {
        var booking = await bookingRepo.Query()
            .Include(b => b.ServiceCategory)
            .Include(b => b.Notifications.OrderByDescending(n => n.CreatedAt))
            .FirstOrDefaultAsync(b => b.Id == id);

        return booking == null ? null : MapToDetailDto(booking);
    }

    public async Task AcceptAsync(int id, DateTime arrivalTime)
    {
        var booking = await bookingRepo.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.Pending) return;

        booking.Status = BookingStatus.Accepted;
        booking.EstimatedArrivalTime = DateTime.SpecifyKind(arrivalTime, DateTimeKind.Utc);
        booking.UpdatedAt = DateTime.UtcNow;
        AddNotification(booking, "Booking accepted — technician will arrive at the estimated time.");
        await db.SaveChangesAsync();
    }

    public async Task DeclineAsync(int id, string reason)
    {
        var booking = await bookingRepo.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.Pending) return;

        booking.Status = BookingStatus.Declined;
        booking.DeclineReason = reason;
        booking.UpdatedAt = DateTime.UtcNow;
        AddNotification(booking, $"Booking declined — {reason}");
        await db.SaveChangesAsync();
    }

    public async Task StartInProgressAsync(int id)
    {
        var booking = await bookingRepo.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.Accepted) return;

        booking.Status = BookingStatus.InProgress;
        booking.UpdatedAt = DateTime.UtcNow;
        AddNotification(booking, "Service in progress — technician is on site.");
        await db.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id)
    {
        var booking = await bookingRepo.FindAsync(id);
        if (booking == null || booking.Status != BookingStatus.InProgress) return;

        booking.Status = BookingStatus.Completed;
        booking.UpdatedAt = DateTime.UtcNow;
        AddNotification(booking, "Service completed. Thank you for choosing ServiceCity!");
        await db.SaveChangesAsync();
    }

    public async Task<List<CustomerListItemDto>> GetCustomersAsync(string? search)
    {
        var query = userRepo.Query().Where(u => !u.IsAdmin);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Name.Contains(term) ||
                u.PhoneNumber.Contains(term) ||
                (u.PhoneNumberNormalized != null && u.PhoneNumberNormalized.Contains(term)));
        }

        var customers = await query.ToListAsync();
        var dtos = new List<CustomerListItemDto>();
        foreach (var c in customers)
        {
            var bookingCount = await bookingRepo.Query().CountAsync(b => b.UserId == c.Id);
            var lastBooking = await bookingRepo.Query()
                .Where(b => b.UserId == c.Id)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => (DateTime?)b.CreatedAt)
                .FirstOrDefaultAsync();

            dtos.Add(new CustomerListItemDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                BookingCount = bookingCount,
                LastBooking = lastBooking
            });
        }

        return dtos.OrderByDescending(c => c.BookingCount).ThenBy(c => c.Name).ToList();
    }

    public async Task<(UserDto? Customer, List<BookingListItemDto> Bookings)> GetCustomerDetailAsync(int id)
    {
        var user = await userRepo.FindAsync(id);
        if (user == null || user.IsAdmin)
            return (null, new List<BookingListItemDto>());

        var bookings = await bookingRepo.GetByUserIdAsync(id);
        var bookingDtos = bookings.Select(MapToListItem).ToList();

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            IsAdmin = user.IsAdmin,
            CreatedAt = user.CreatedAt
        };

        return (userDto, bookingDtos);
    }

    private async Task<List<Booking>> QueryBookings(string? search, string? status)
    {
        IQueryable<Booking> query = bookingRepo.Query().Include(b => b.ServiceCategory);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsed))
            query = query.Where(b => b.Status == parsed);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b =>
                b.ReferenceNumber.Contains(term) ||
                b.CustomerPhone.Contains(term) ||
                (b.CustomerPhoneNormalized != null && b.CustomerPhoneNormalized.Contains(term)) ||
                b.CustomerName.Contains(term));
        }

        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
    }

    private static AdminDashboardDto BuildDashboardDto(List<Booking> bookings, string? search, string? status) => new()
    {
        Search = search,
        StatusFilter = status,
        GroupedBookings = bookings.GroupBy(b => b.Status)
            .ToDictionary(g => g.Key, g => g.Select(MapToListItem).ToList()),
        Counts = bookings.GroupBy(b => b.Status)
            .ToDictionary(g => g.Key, g => g.Count())
    };

    private static BookingDetailDto MapToDetailDto(Booking b) => new()
    {
        Id = b.Id,
        ReferenceNumber = b.ReferenceNumber,
        CustomerName = b.CustomerName,
        CustomerPhone = b.CustomerPhone,
        Address = b.Address,
        Description = b.Description,
        ServiceCategoryName = b.ServiceCategory?.Name ?? "",
        Status = b.Status,
        PreferredDate = b.PreferredDate,
        PreferredTimeSlot = b.PreferredTimeSlot,
        CreatedAt = b.CreatedAt,
        EstimatedArrivalTime = b.EstimatedArrivalTime,
        DeclineReason = b.DeclineReason,
        Notifications = b.Notifications.OrderByDescending(n => n.CreatedAt).Select(n => new NotificationDto
        {
            Message = n.Message,
            StatusTo = n.StatusTo,
            CreatedAt = n.CreatedAt
        }).ToList()
    };

    private static BookingListItemDto MapToListItem(Booking b) => new()
    {
        Id = b.Id,
        ReferenceNumber = b.ReferenceNumber,
        CustomerName = b.CustomerName,
        ServiceCategoryName = b.ServiceCategory?.Name ?? "",
        Status = b.Status,
        PreferredDate = b.PreferredDate,
        PreferredTimeSlot = b.PreferredTimeSlot,
        CreatedAt = b.CreatedAt
    };

    private void AddNotification(Booking booking, string message)
    {
        notificationRepo.Add(new Notification
        {
            BookingId = booking.Id,
            Message = message,
            StatusTo = booking.Status,
            CreatedAt = DateTime.UtcNow
        });
    }
}
