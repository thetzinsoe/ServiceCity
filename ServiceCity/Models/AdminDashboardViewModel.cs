using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;

namespace ServiceCity.Models;

public class AdminDashboardViewModel
{
    public string? Search { get; set; }
    public Dictionary<BookingStatus, List<Booking>> GroupedBookings { get; set; } = new();
    public Dictionary<BookingStatus, int> Counts { get; set; } = new();
}
