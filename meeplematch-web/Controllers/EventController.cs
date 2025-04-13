using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using meeplematch_web.DTO;
using AutoMapper;
using meeplematch_web.Models;
using meeplematch_web.Interfaces;

namespace meeplematch_web.Controllers
{
    //[Route("Event/[action]/{id?}")]
    public class EventController : Controller
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventApiService _eventService;
        private readonly IUserApiService _userService;
        private readonly IMapper _mapper;

        public EventController(ILogger<EventController> logger, IEventApiService eventService, 
            IUserApiService userService, IMapper mapper)
        {
            // PR TEST
            _logger = logger;
            _eventService = eventService;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index(string search = "", int page = 1, int pageSize = 12)
        {
            var allEvents = await _eventService.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                allEvents = allEvents
                    .Where(e => e.Name.ToLower().Contains(search.ToLower()) || e.Game.ToLower().Contains(search.ToLower()))
                    .ToList();
            }

            var pagedEvents = allEvents
                .OrderBy(e => e.EventDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModels = _mapper.Map<List<EventViewModel>>(pagedEvents);

            foreach (var ev in viewModels)
            {
                var publicUser = await _userService.GetPublicUserAsync(ev.CreatedBy);
                ev.CreatedByNavigation = new UserViewModel { Username = publicUser?.Username ?? "Unknown" };
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)allEvents.Count / pageSize);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EventListPartial", viewModels);

            return View(viewModels);
        }

        // GET: EventController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _eventService.GetByIdAsync(id);
            if (dto == null) return NotFound();

            var viewModel = _mapper.Map<EventViewModel>(dto);
            var publicUser = await _userService.GetPublicUserAsync(viewModel.CreatedBy);
            viewModel.CreatedByNavigation = new UserViewModel { Username = publicUser?.Username ?? "Unknown" };

            return View(viewModel);
        }

        // GET: EventController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel viewModel, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                viewModel.ImagePath = $"/assets/pic_uploads/{fileName}";
            }

            var dto = _mapper.Map<EventDTO>(viewModel);
            dto.CreatedAt = DateTime.UtcNow;
            dto.CreatedBy = 2; // temporary until login is done

            await _eventService.CreateAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        // GET: EventController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _eventService.GetByIdAsync(id);
            if (dto == null) return NotFound();

            var viewModel = _mapper.Map<EventViewModel>(dto);
            return View(viewModel);
        }

        // POST: EventController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel viewModel, IFormFile image)
        {
            if (!ModelState.IsValid) return View(viewModel);

            if (image != null && image.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                    await image.CopyToAsync(stream);

                viewModel.ImagePath = $"/assets/pic_uploads/{fileName}";
            }

            var dto = _mapper.Map<EventDTO>(viewModel);
            dto.UpdatedAt = DateTime.UtcNow;

            await _eventService.UpdateAsync(id, dto);

            return RedirectToAction(nameof(Index));
        }
    }
}
