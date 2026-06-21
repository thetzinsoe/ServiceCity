using System.Security.Claims;
using ServiceCity.Services.DTOs;
using ServiceCity.Services.DTOs.Requests;

namespace ServiceCity.Services.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request, int? userId);
    Task<BookingDto?> GetConfirmationAsync(string referenceNumber);
    Task<BookingDto?> GetStatusAsync(string referenceNumber);
    Task<List<BookingListItemDto>> GetMyBookingsAsync(int userId);
}

public interface IAuthService
{
    Task<(UserDto? User, string? Error)> SignInAsync(SignInRequest request);
    Task<(UserDto? User, string? Error)> RegisterAsync(RegisterRequest request);
    Task<(UserDto? User, string? Error)> SetupAsync(SetupRequest request);
    ClaimsPrincipal CreatePrincipal(UserDto user);
    Task<(UserDto? User, string? Error)> UpdateSettingsAsync(int userId, SettingsRequest request);
    Task<UserDto?> GetUserAsync(int userId);
    Task<bool> AdminExistsAsync();
}

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync(string? search, string? status);
    Task<AdminDashboardDto> GetDrilldownAsync(string status, string? search);
    Task<BookingDetailDto?> GetBookingDetailAsync(int id);
    Task AcceptAsync(int id, DateTime arrivalTime);
    Task DeclineAsync(int id, string reason);
    Task StartInProgressAsync(int id);
    Task CompleteAsync(int id);
    Task<List<CustomerListItemDto>> GetCustomersAsync(string? search);
    Task<(UserDto? Customer, List<BookingListItemDto> Bookings)> GetCustomerDetailAsync(int id);
}
