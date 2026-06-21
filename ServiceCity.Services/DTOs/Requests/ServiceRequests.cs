using System.ComponentModel.DataAnnotations;
using ServiceCity.Core.Enums;

namespace ServiceCity.Services.DTOs.Requests;

public class CreateBookingRequest
{
    [Required]
    public int ServiceCategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime PreferredDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    public PreferredTimeSlot PreferredTimeSlot { get; set; }
}

public class SignInRequest
{
    [Required(ErrorMessage = "Phone number is required.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class SetupRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Name { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    public string? PhoneNumber { get; set; }
}

public class SettingsRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}
