using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ServiceCity.Services.DTOs.Requests;
using ServiceCity.Services.Interfaces;

namespace ServiceCity.Controllers;

public class AuthController(IAuthService authService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Setup()
    {
        if (await authService.AdminExistsAsync())
            return NotFound();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("AuthRegister")]
    public async Task<IActionResult> Setup(SetupRequest model)
    {
        if (await authService.AdminExistsAsync())
            return NotFound();

        if (!ModelState.IsValid) return View(model);

        var (user, error) = await authService.SetupAsync(model);
        if (error != null)
        {
            if (error.Contains("Password") && !error.Contains("Passwords"))
                ModelState.AddModelError("Password", error);
            else if (error.Contains("Passwords"))
                ModelState.AddModelError("ConfirmPassword", error);
            else
                ModelState.AddModelError("PhoneNumber", error);
            return View(model);
        }

        TempData["SuccessMessage"] = "Admin account created. Please sign in.";
        return RedirectToAction("SignIn");
    }

    [HttpGet]
    public IActionResult SignIn()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("AuthSignIn")]
    public async Task<IActionResult> SignIn(SignInRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        var (user, error) = await authService.SignInAsync(model);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        var principal = authService.CreatePrincipal(user!);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return user!.IsAdmin
            ? RedirectToAction("Dashboard", "Admin")
            : RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("AuthRegister")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        var (user, error) = await authService.RegisterAsync(model);
        if (error != null)
        {
            ModelState.AddModelError("PhoneNumber", error);
            return View(model);
        }

        var principal = authService.CreatePrincipal(user!);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public new async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("SignIn");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Settings()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("SignIn");

        var user = await authService.GetUserAsync(userId.Value);
        if (user == null) return RedirectToAction("SignIn");

        return View(new SettingsRequest
        {
            Name = user.Name,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(SettingsRequest model)
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("SignIn");

        if (!ModelState.IsValid) return View(model);

        var (_, error) = await authService.UpdateSettingsAsync(userId.Value, model);
        if (error != null)
        {
            ModelState.AddModelError("PhoneNumber", error);
            return View(model);
        }

        TempData["SuccessMessage"] = "Settings saved.";
        return RedirectToAction("Settings");
    }

    private int? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim != null && int.TryParse(claim, out var id) ? id : null;
    }
}
