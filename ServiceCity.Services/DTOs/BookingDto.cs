using ServiceCity.Core.Enums;

namespace ServiceCity.Services.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServiceCategoryName { get; set; } = string.Empty;
    public DateTime PreferredDate { get; set; }
    public PreferredTimeSlot PreferredTimeSlot { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public string? DeclineReason { get; set; }
    public List<NotificationDto> Notifications { get; set; } = [];
}
