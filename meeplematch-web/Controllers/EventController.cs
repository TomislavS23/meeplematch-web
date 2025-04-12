using Microsoft.AspNetCore.Mvc;
using meeplematch_api.Model;
using meeplematch_api.Repository;
using meeplematch_api.DTO;
using AutoMapper;
using meeplematch_web.Models;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace meeplematch_web.Controllers
{
    //[Route("Event/[action]/{id?}")]
    public class EventController : Controller
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        public EventController(ILogger<EventController> logger, IEventRepository eventRepository, IUserRepository userRepository, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index(string search = "", int page = 1, int pageSize = 3)
        {
            var httpClient = _httpClientFactory.CreateClient("MeepleMatch");
                HttpResponseMessage response = httpClient.GetAsync("events").Result;
                if (response.IsSuccessStatusCode)
                {
                    var events = string.IsNullOrEmpty(search)
                        ? response.Content.ReadAsAsync<List<EventViewModel>>().Result
                        : response.Content.ReadAsAsync<List<EventViewModel>>().Result
                            .Where(e => e.Name.ToLower().Contains(search.ToLower()) || e.Game.ToLower().Contains(search.ToLower()))
                            .ToList();

                    var pagedEvents = events
                        .OrderBy(e => e.EventDate)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

                    foreach (var ev in pagedEvents)
                    {
                        var userDto = _userRepository.GetUser(ev.CreatedBy);
                        ev.CreatedByNavigation = new User
                        {
                            Username = userDto.Username
                        };
                    }

                    ViewBag.CurrentPage = page;
                    ViewBag.TotalPages = (int)Math.Ceiling((double)events.Count / pageSize);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return PartialView("_EventListPartial", pagedEvents);
                    }
                    return View(pagedEvents);
                }

            return StatusCode(500);
        }

        // GET: EventController/Details/5
        public IActionResult Details(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("MeepleMatch");
            HttpResponseMessage response = httpClient.GetAsync($"events/{id}").Result;
                if (response.IsSuccessStatusCode)
                {
                    var @event = response.Content.ReadAsAsync<EventViewModel>().Result;
                    return View(@event);
                }

            return NotFound();
        }

        // GET: EventController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel viewModel, IFormFile? image)
        {

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    var field = kvp.Key;
                    var errors = kvp.Value.Errors;
                    foreach (var error in errors)
                    {
                        _logger.LogError($"Validation error in '{field}': {error.ErrorMessage}");
                    }
                }

                return View(viewModel);
            }

            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                viewModel.ImagePath = $"/assets/pic_uploads/{uniqueFileName}";
            }

            viewModel.CreatedBy = 1;
            viewModel.CreatedAt = DateTime.UtcNow;

            var postEvent = new StringContent(
                JsonSerializer.Serialize(viewModel),
                Encoding.UTF8,
                Application.Json);
            var httpClient = _httpClientFactory.CreateClient("MeepleMatch");
            using var response = await httpClient.PostAsync("events", postEvent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: EventController/Edit/5
        public IActionResult Edit(int id)
        {
            var @event = _eventRepository.FindById(id);
            if (@event is null) return NotFound();
            var eventViewModel = _mapper.Map<EventViewModel>(@event);
            return View(eventViewModel);
        }

        // POST: EventController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, EventViewModel viewModel, IFormFile image)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var existingEvent = _eventRepository.FindById(id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }

                viewModel.ImagePath = $"/assets/pic_uploads/{uniqueFileName}";
            }
            else
            {
                viewModel.ImagePath = existingEvent.ImagePath;
            }

            var eventDto = _mapper.Map<EventDTO>(viewModel);
            eventDto.UpdatedAt = DateTime.UtcNow;

            _eventRepository.Update(eventDto, id);

            return RedirectToAction(nameof(Index));
        }


        // GET: EventController/Delete/5
        public IActionResult Delete(int id)
        {
            var @event = _eventRepository.FindById(id);
            if (@event is null) return NotFound();
            var eventViewModel = _mapper.Map<EventViewModel>(@event);
            return View(eventViewModel);
        }

        // POST: EventController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, EventDTO eventDTO)
        {
            try
            {
                if (_eventRepository.FindById(id) is null) return NotFound();
                _eventRepository.Delete(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //return RedirectToAction(nameof(Details), id);
            }
                return RedirectToAction(nameof(Index));
        }



        // GET: EventController/Delete/5
        public IActionResult Delete2(int id)
        {
            var @event = _eventRepository.FindById(id);
            if (@event is null) return NotFound();
            var eventViewModel = _mapper.Map<EventViewModel>(@event);
            return View(eventViewModel);
        }

        // POST: EventController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int id, EventDTO eventDTO)
        {
            try
            {
                if (_eventRepository.FindById(id) is null) return NotFound();
                _eventRepository.Delete(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                //return RedirectToAction(nameof(Details), id);
            }
                return RedirectToAction(nameof(Index));
        }
    }
}
