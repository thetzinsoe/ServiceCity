using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using ServiceCity.Data.Interfaces;
using ServiceCity.Services.DTOs.Requests;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Controllers;

public class BookingController(
    IBookingService bookingService,
    IAuthService authService,
    IServiceCategoryRepository categoryRepo) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create(int serviceCategoryId)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            TempData["InfoMessage"] = "Sign in or create an account to book.";
            return RedirectToAction("SignIn", "Auth");
        }

        var category = await categoryRepo.FindAsync(serviceCategoryId);
        if (category == null) return NotFound();

        var model = new CreateBookingRequest
        {
            ServiceCategoryId = serviceCategoryId
        };

        // Pre-fill identity from logged-in user
        var userId = GetUserId();
        if (userId != null)
        {
            var user = await authService.GetUserAsync(userId.Value);
            if (user != null)
            {
                model.CustomerName = user.Name;
                model.CustomerPhone = user.PhoneNumber;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("BookingSubmission")]
    public async Task<IActionResult> Create(CreateBookingRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var userId = GetUserId();
            var booking = await bookingService.CreateBookingAsync(model, userId);
            return RedirectToAction("Confirmation", new { referenceNumber = booking.ReferenceNumber });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("CustomerPhone", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [EnableRateLimiting("BookingStatus")]
    public async Task<IActionResult> Confirmation(string referenceNumber)
    {
        var booking = await bookingService.GetConfirmationAsync(referenceNumber);
        if (booking == null) return NotFound();
        return View(booking);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("SignIn", "Auth");

        var bookings = await bookingService.GetMyBookingsAsync(userId.Value);
        return View(bookings);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Lookup()
    {
        return RedirectToAction("MyBookings");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult Lookup(string phoneNumber)
    {
        return RedirectToAction("MyBookings");
    }

    [HttpGet]
    [EnableRateLimiting("BookingStatus")]
    public async Task<IActionResult> Status(string referenceNumber)
    {
        var booking = await bookingService.GetStatusAsync(referenceNumber);
        if (booking == null) return NotFound();
        return View(booking);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim != null && int.TryParse(claim, out var id) ? id : null;
    }
}
