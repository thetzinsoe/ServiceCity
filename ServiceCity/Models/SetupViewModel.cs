using System.ComponentModel.DataAnnotations;

namespace ServiceCity.Models;

public class SetupViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }
}
