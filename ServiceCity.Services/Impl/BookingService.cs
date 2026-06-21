using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;
using ServiceCity.Data;
using ServiceCity.Data.Interfaces;
using ServiceCity.Services.DTOs;
using ServiceCity.Services.DTOs.Requests;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Services.Impl;

public class BookingService(
    AppDbContext db,
    IBookingRepository bookingRepo,
    INotificationRepository notificationRepo,
    IPhoneValidationService phoneValidator) : IBookingService
{
    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, int? userId)
    {
        var (isValid, normalized, error) = phoneValidator.Validate(request.CustomerPhone);
        if (!isValid)
            throw new ArgumentException(error, nameof(request.CustomerPhone));

        var idempotencyKey = GenerateIdempotencyKey(
            normalized!, request.ServiceCategoryId, request.PreferredDate,
            request.CustomerName, request.Address);

        var existing = await bookingRepo.Query()
            .FirstOrDefaultAsync(b => b.IdempotencyKey == idempotencyKey);
        if (existing != null)
            return MapToDto(existing);

        var referenceNumber = GenerateReferenceNumber();
        var booking = new Booking
        {
            ReferenceNumber = referenceNumber,
            ServiceCategoryId = request.ServiceCategoryId,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerPhoneNormalized = normalized,
            Address = request.Address,
            Description = request.Description,
            PreferredDate = DateTime.SpecifyKind(request.PreferredDate, DateTimeKind.Utc),
            PreferredTimeSlot = request.PreferredTimeSlot,
            Status = BookingStatus.Pending,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        bookingRepo.Add(booking);
        notificationRepo.Add(new Notification
        {
            Booking = booking,
            Message = "Booking created — your request has been received.",
            StatusTo = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<BookingDto?> GetConfirmationAsync(string referenceNumber)
    {
        var booking = await bookingRepo.GetByReferenceAsync(referenceNumber);
        if (booking == null) return null;

        // Load notifications for the timeline
        var notifications = await notificationRepo.GetByBookingIdAsync(booking.Id);
        return MapToDto(booking, notifications);
    }

    public async Task<BookingDto?> GetStatusAsync(string referenceNumber)
    {
        var booking = await bookingRepo.GetByReferenceAsync(referenceNumber);
        if (booking == null) return null;

        // Load notifications for the timeline
        var notifications = await notificationRepo.GetByBookingIdAsync(booking.Id);
        return MapToDto(booking, notifications);
    }

    public async Task<List<BookingListItemDto>> GetMyBookingsAsync(int userId)
    {
        var bookings = await bookingRepo.GetByUserIdAsync(userId);
        return bookings.Select(MapToListItem).ToList();
    }

    private static BookingDto MapToDto(Booking b, List<Notification>? notifications = null) => new()
    {
        Id = b.Id,
        ReferenceNumber = b.ReferenceNumber,
        CustomerName = b.CustomerName,
        CustomerPhone = b.CustomerPhone,
        Address = b.Address,
        Description = b.Description,
        ServiceCategoryName = b.ServiceCategory?.Name ?? "",
        PreferredDate = b.PreferredDate,
        PreferredTimeSlot = b.PreferredTimeSlot,
        Status = b.Status,
        CreatedAt = b.CreatedAt,
        EstimatedArrivalTime = b.EstimatedArrivalTime,
        DeclineReason = b.DeclineReason,
        Notifications = notifications?.Select(n => new NotificationDto
        {
            Message = n.Message,
            StatusTo = n.StatusTo,
            CreatedAt = n.CreatedAt
        }).ToList() ?? []
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

    private static string GenerateReferenceNumber()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = RandomNumberGenerator.GetBytes(10);
        var suffix = new char[10];
        for (int i = 0; i < 10; i++)
            suffix[i] = chars[bytes[i] % chars.Length];
        return $"SC-{new string(suffix)}";
    }

    private static string GenerateIdempotencyKey(string phone, int categoryId, DateTime date, string name, string address)
    {
        var data = $"{phone}-{categoryId}-{date:yyyyMMdd}-{name}-{address}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(bytes);
    }
}
