using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using meeplematch_web.Models;

namespace meeplematch_web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        // test
        _logger = logger;
    }

    public IActionResult Index()
    {
        // test
        return View();
    }
    public IActionResult About()
    {
        // test
        return View();
    }

}