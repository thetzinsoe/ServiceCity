using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Data;
using ServiceCity.Models;
using ServiceCity.Core.Entities;
using PhoneNumbers;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ServiceCity.Controllers;

public class AuthController(AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        var hasAdmin = await db.Users.AnyAsync(u => u.IsAdmin);
        if (hasAdmin) return NotFound();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Setup(SetupViewModel model)
    {
        var hasAdmin = await db.Users.AnyAsync(u => u.IsAdmin);
        if (hasAdmin) return NotFound();

        if (!ModelState.IsValid) return View(model);

        if (model.Password.Length < 6)
        {
            ModelState.AddModelError("Password", "Password must be at least 6 characters.");
            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            return View(model);
        }

        string? normalizedPhone = null;
        if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            var (isValid, normalized, error) = ValidateAndNormalizePhone(model.PhoneNumber);
            if (!isValid)
            {
                ModelState.AddModelError("PhoneNumber", error!);
                return View(model);
            }
            normalizedPhone = normalized;
        }

        var hasher = new PasswordHasher<User>();
        var user = new User
        {
            Username = model.Username,
            PasswordHash = hasher.HashPassword(null!, model.Password),
            Name = model.Name ?? model.Username,
            PhoneNumber = model.PhoneNumber ?? "",
            PhoneNumberNormalized = normalizedPhone,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Admin account created. Please sign in.";
        return RedirectToAction("SignIn");
    }

    private static (bool IsValid, string? Normalized, string? Error) ValidateAndNormalizePhone(string input)
    {
        var util = PhoneNumberUtil.GetInstance();
        try
        {
            var number = util.Parse(input, "MM");
            if (!util.IsValidNumber(number))
            {
                return (false, null, "Please enter a valid Myanmar phone number.");
            }
            var normalized = util.Format(number, PhoneNumberFormat.E164);
            return (true, normalized, null);
        }
        catch (NumberParseException)
        {
            return (false, null, "Please enter a valid Myanmar phone number (e.g., 09-123-456-789 or +959123456789).");
        }
    }
}
