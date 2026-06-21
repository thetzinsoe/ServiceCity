using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Core.Entities;
using ServiceCity.Data;
using ServiceCity.Data.Interfaces;
using ServiceCity.Services.DTOs;
using ServiceCity.Services.DTOs.Requests;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Services.Impl;

public class AuthService(
    AppDbContext db,
    IUserRepository userRepo,
    IPhoneValidationService phoneValidator) : IAuthService
{
    // Prevents concurrent admin setup (race condition)
    private static readonly SemaphoreSlim SetupLock = new(1, 1);

    public async Task<(UserDto? User, string? Error)> SignInAsync(SignInRequest request)
    {
        var (isValid, normalized, error) = phoneValidator.Validate(request.PhoneNumber);
        if (!isValid)
            return (null, error);

        var user = await userRepo.GetByPhoneAsync(normalized!)
                   ?? await userRepo.GetByUsernameAsync(request.PhoneNumber);

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return (null, "Invalid phone number or password.");

        // Check for account lockout after repeated failures
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            return (null, $"Account locked. Try again after {user.LockedUntil.Value.ToLocalTime():HH:mm}.");

        var hasher = new PasswordHasher<User>();
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLoginAttempts++;
            user.LastFailedLoginAt = DateTime.UtcNow;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                await db.SaveChangesAsync();
                return (null, "Account locked due to repeated failures. Try again in 15 minutes.");
            }

            await db.SaveChangesAsync();
            return (null, "Invalid phone number or password.");
        }

        // Reset failed attempts on successful login
        if (user.FailedLoginAttempts > 0 || user.LockedUntil != null)
        {
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
            user.LastFailedLoginAt = null;
            await db.SaveChangesAsync();
        }

        return (MapToDto(user), null);
    }

    public async Task<(UserDto? User, string? Error)> RegisterAsync(RegisterRequest request)
    {
        var (isValid, normalized, error) = phoneValidator.Validate(request.PhoneNumber);
        if (!isValid)
            return (null, error);

        if (await userRepo.PhoneExistsAsync(normalized!))
            return (null, "An account with this phone number already exists.");

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            PhoneNumberNormalized = normalized,
            Username = request.PhoneNumber,
            PasswordHash = hasher.HashPassword(null!, request.Password),
            Address = request.Address,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };

        userRepo.Add(user);
        await db.SaveChangesAsync();

        return (MapToDto(user), null);
    }

    public async Task<(UserDto? User, string? Error)> SetupAsync(SetupRequest request)
    {
        // Prevent concurrent admin creation via server-side lock
        await SetupLock.WaitAsync();
        try
        {
            if (await AdminExistsAsync())
                return (null, "Admin account already exists.");
        }
        finally
        {
            SetupLock.Release();
        }

        if (request.Password != request.ConfirmPassword)
            return (null, "Passwords do not match.");

        string? normalizedPhone = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var (isValid, normalized, error) = phoneValidator.Validate(request.PhoneNumber);
            if (!isValid)
                return (null, error);
            normalizedPhone = normalized;
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = request.Username,
            PasswordHash = hasher.HashPassword(null!, request.Password),
            Name = request.Name ?? request.Username,
            PhoneNumber = request.PhoneNumber ?? "",
            PhoneNumberNormalized = normalizedPhone,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        userRepo.Add(user);
        await db.SaveChangesAsync();

        return (MapToDto(user), null);
    }

    public ClaimsPrincipal CreatePrincipal(UserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
        };
        if (user.IsAdmin)
            claims.Add(new(ClaimTypes.Role, "Admin"));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    public async Task<(UserDto? User, string? Error)> UpdateSettingsAsync(int userId, SettingsRequest request)
    {
        var user = await userRepo.FindAsync(userId);
        if (user == null)
            return (null, "User not found.");

        string? normalizedPhone = null;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var (isValid, normalized, error) = phoneValidator.Validate(request.PhoneNumber);
            if (!isValid)
                return (null, error);
            normalizedPhone = normalized;
        }

        user.Name = request.Name;
        user.PhoneNumber = request.PhoneNumber ?? "";
        user.PhoneNumberNormalized = normalizedPhone;
        await db.SaveChangesAsync();

        return (MapToDto(user), null);
    }

    public async Task<UserDto?> GetUserAsync(int userId)
    {
        var user = await userRepo.FindAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<bool> AdminExistsAsync()
        => await userRepo.Query().AnyAsync(u => u.IsAdmin);

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address,
        IsAdmin = user.IsAdmin,
        CreatedAt = user.CreatedAt
    };
}
