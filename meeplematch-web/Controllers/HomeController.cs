using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using meeplematch_web.Models;
using AutoMapper;
using meeplematch_web.Utils;

namespace meeplematch_web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpClientFactory _httpClientFactory;

    public HomeController(ILogger<HomeController> logger, IMapper mapper, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _mapper = mapper;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
        var response = await httpClient.GetAsync("events");
        if (response.IsSuccessStatusCode)
        {
            var events = await response.Content.ReadAsAsync<List<EventViewModel>>();
            var latestEvents = events
                .OrderByDescending(e => e.CreatedAt)
                .Take(9)
                .ToList();
            return View(latestEvents);
        }
        else
        {
            return View(new List<EventViewModel>());
        }
    }
    public IActionResult About()
    {
        return View();
    }

}