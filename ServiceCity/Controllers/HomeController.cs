using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceCity.Data;
using ServiceCity.Models;
using System.Diagnostics;

namespace ServiceCity.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            return RedirectToAction("Dashboard", "Admin");

        var categories = await db.ServiceCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        return View(categories);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
