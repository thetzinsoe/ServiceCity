namespace ServiceCity.Services.DTOs;

public class BookingListItemDto
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ServiceCategoryName { get; set; } = string.Empty;
    public Core.Enums.BookingStatus Status { get; set; }
    public DateTime PreferredDate { get; set; }
    public Core.Enums.PreferredTimeSlot PreferredTimeSlot { get; set; }
    public DateTime CreatedAt { get; set; }
}
