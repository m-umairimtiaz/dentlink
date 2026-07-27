using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UniversityCompanyAppointmentSystem.Common;
using UniversityCompanyAppointmentSystem.Models;

namespace UniversityCompanyAppointmentSystem.Controllers;

// The public "front door" of the site: a welcome page with Login/Register links.
// If someone is already logged in, we send them straight to their dashboard instead.
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Already logged in? Skip the welcome page and go straight to the right dashboard.
        if (HttpContext.Session.GetInt32(SessionKeys.CompanyId) != null)
        {
            return RedirectToAction("Index", "CompanyDashboard");
        }
        if (HttpContext.Session.GetInt32(SessionKeys.UniversityId) != null)
        {
            return RedirectToAction("Index", "UniversityDashboard");
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
