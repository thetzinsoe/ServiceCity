namespace ServiceCity.Services.DTOs;

public class BookingDetailDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServiceCategoryName { get; set; } = string.Empty;
    public Core.Enums.BookingStatus Status { get; set; }
    public DateTime PreferredDate { get; set; }
    public Core.Enums.PreferredTimeSlot PreferredTimeSlot { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedArrivalTime { get; set; }
    public string? DeclineReason { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
}
