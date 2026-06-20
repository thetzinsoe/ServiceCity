namespace ServiceCity.Models;

public class CustomerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public DateTime? LastBooking { get; set; }
}
