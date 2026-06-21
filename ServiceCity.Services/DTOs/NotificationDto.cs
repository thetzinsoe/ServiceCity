using ServiceCity.Core.Enums;

namespace ServiceCity.Services.DTOs;

public class NotificationDto
{
    public string Message { get; set; } = string.Empty;
    public BookingStatus? StatusTo { get; set; }
    public DateTime CreatedAt { get; set; }
}
