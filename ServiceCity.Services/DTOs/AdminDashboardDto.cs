using ServiceCity.Core.Enums;

namespace ServiceCity.Services.DTOs;

public class AdminDashboardDto
{
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public Dictionary<BookingStatus, List<BookingListItemDto>> GroupedBookings { get; set; } = new();
    public Dictionary<BookingStatus, int> Counts { get; set; } = new();
}
