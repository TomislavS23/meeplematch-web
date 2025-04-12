using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using meeplematch_web.Models;
using AutoMapper;
using meeplematch_api.Repository;

namespace meeplematch_web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;

    public HomeController(ILogger<HomeController> logger, IEventRepository eventRepository, IMapper mapper)
    {
        _logger = logger;
        _eventRepository = eventRepository;
        _mapper = mapper;
    }

    public IActionResult Index()
    {
        var latestEvents = _eventRepository.FindAll()
            .OrderByDescending(e => e.CreatedAt)
            .Take(9)
            .ToList();

        var viewModel = _mapper.Map<List<EventViewModel>>(latestEvents);

        return View(viewModel);
    }
    public IActionResult About()
    {
        return View();
    }

}