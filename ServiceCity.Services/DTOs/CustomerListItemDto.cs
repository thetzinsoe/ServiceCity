namespace ServiceCity.Services.DTOs;

public class CustomerListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public DateTime? LastBooking { get; set; }
}
