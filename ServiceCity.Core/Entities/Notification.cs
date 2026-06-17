using ServiceCity.Core.Enums;

namespace ServiceCity.Core.Entities;

public class Notification
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public BookingStatus? StatusFrom { get; set; }
    public BookingStatus? StatusTo { get; set; }
    public bool IsViewed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
