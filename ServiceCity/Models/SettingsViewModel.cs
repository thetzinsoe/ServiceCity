using System.ComponentModel.DataAnnotations;

namespace ServiceCity.Models;

public class SettingsViewModel
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }
}
