using Microsoft.AspNetCore.Mvc;
using meeplematch_web.Interfaces;
using meeplematch_web.Models;
using meeplematch_web.DTO;
using AutoMapper;

namespace meeplematch_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventApiService _eventService;
        private readonly IUserApiService _userService;
        private readonly IMapper _mapper;

        public HomeController(ILogger<HomeController> logger, IEventApiService eventService,
            IUserApiService userService, IMapper mapper) 
        {
            _logger = logger;
            _eventService = eventService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var allEvents = await _eventService.GetAllAsync();

            var latestEvents = allEvents
                .OrderByDescending(e => e.CreatedAt)
                .Take(9)
                .ToList();

            var viewModels = _mapper.Map<List<EventViewModel>>(latestEvents);

            foreach (var ev in viewModels)
            {
                var publicUser = await _userService.GetPublicUserAsync(ev.CreatedBy);
                ev.CreatedByNavigation = _mapper.Map<UserViewModel>(publicUser ?? new PublicUserDTO { Username = "Unknown" });
            }

            return View(viewModels);
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
