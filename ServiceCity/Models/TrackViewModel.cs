using System.ComponentModel.DataAnnotations;

namespace ServiceCity.Models;

public class TrackViewModel
{
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
}
