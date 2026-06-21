using Microsoft.AspNetCore.Mvc;
using ServiceCity.Data.Interfaces;
using ServiceCity.Models;

namespace ServiceCity.Controllers;

public class HomeController(IServiceCategoryRepository categoryRepo) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            return RedirectToAction("Dashboard", "Admin");

        var categories = await categoryRepo.GetAllAsync();
        return View(categories);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
