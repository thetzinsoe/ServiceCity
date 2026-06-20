using System.ComponentModel.DataAnnotations;
using ServiceCity.Core.Enums;

namespace ServiceCity.Models;

public class BookingViewModel
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
