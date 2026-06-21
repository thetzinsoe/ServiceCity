using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService) : Controller
{
    public async Task<IActionResult> Dashboard(string? search, string? status)
    {
        var dto = await adminService.GetDashboardAsync(search, status);
        return View(dto);
    }

    public async Task<IActionResult> Drilldown(string status, string? search)
    {
        if (string.IsNullOrWhiteSpace(status))
            return RedirectToAction("Dashboard");

        var dto = await adminService.GetDrilldownAsync(status, search);
        return View(dto);
    }

    public async Task<IActionResult> Details(int id)
    {
        var dto = await adminService.GetBookingDetailAsync(id);
        if (dto == null) return NotFound();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id, DateTime arrivalTime)
    {
        await adminService.AcceptAsync(id, arrivalTime);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(int id, string reason)
    {
        await adminService.DeclineAsync(id, reason);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InProgress(int id)
    {
        await adminService.StartInProgressAsync(id);
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        await adminService.CompleteAsync(id);
        return RedirectToAction("Details", new { id });
    }

    public async Task<IActionResult> Customers(string? search)
    {
        var customers = await adminService.GetCustomersAsync(search);
        return View(customers);
    }

    public async Task<IActionResult> CustomerDetail(int id)
    {
        var (customer, bookings) = await adminService.GetCustomerDetailAsync(id);
        if (customer == null) return NotFound();

        ViewBag.Customer = customer;
        return View(bookings);
    }
}
