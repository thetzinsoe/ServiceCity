using ServiceCity.Core.Enums;

namespace ServiceCity.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ServiceCategoryId { get; set; }
    public ServiceCategory ServiceCategory { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime PreferredDate { get; set; }
    public PreferredTimeSlot PreferredTimeSlot { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
