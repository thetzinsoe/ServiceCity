using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ServiceCity.Controllers;

[Authorize]
public class AdminController : Controller
{
    public IActionResult Dashboard()
    {
        return View();
    }
}
