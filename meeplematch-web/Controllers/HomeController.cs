//using Microsoft.AspNetCore.Mvc;
//using meeplematch_web.Interfaces;
//using meeplematch_web.Models;
//using meeplematch_web.DTO;
//using AutoMapper;

//namespace meeplematch_web.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly IEventApiService _eventService;
//        private readonly IUserApiService _userService;
//        private readonly IMapper _mapper;

//        public HomeController(ILogger<HomeController> logger, IEventApiService eventService,
//            IUserApiService userService, IMapper mapper) 
//        {
//            _logger = logger;
//            _eventService = eventService;
//            _userService = userService;
//            _mapper = mapper;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var allEvents = await _eventService.GetAllAsync();

//            var latestEvents = allEvents
//                .OrderByDescending(e => e.CreatedAt)
//                .Take(9)
//                .ToList();

//            var viewModels = _mapper.Map<List<EventViewModel>>(latestEvents);

//            foreach (var ev in viewModels)
//            {
//                var publicUser = await _userService.GetPublicUserAsync(ev.CreatedBy);
//                ev.CreatedByNavigation = _mapper.Map<UserViewModel>(publicUser ?? new PublicUserDTO { Username = "Unknown" });
//            }

//            return View(viewModels);
//        }

//        public IActionResult About()
//        {
//            return View();
//        }
//    }
//}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using meeplematch_web.Models;
using AutoMapper;
using meeplematch_web.Utils;
using meeplematch_web.DTO;

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
            foreach (var ev in events)
            {
                var userResponse = await httpClient.GetAsync($"user/public/{ev.CreatedBy}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var user = await userResponse.Content.ReadAsAsync<PublicUserViewModel>();
                    ev.CreatedByNavigation = _mapper.Map<PublicUserViewModel>(user ?? new PublicUserViewModel { Username = "Unknown" });
                }
            }

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