using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using meeplematch_web.Models;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Linq;
using System.Collections.Specialized;
using NuGet.Protocol;
using meeplematch_web.Utils;

namespace meeplematch_web.Controllers
{
    //[Route("Event/[action]/{id?}")]
    public class EventController : Controller
    {
        private readonly ILogger<EventController> _logger;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string apiUri = "events";

        public EventController(ILogger<EventController> logger, IMapper mapper, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index(string search = "", int page = 1, int pageSize = 3)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            HttpResponseMessage response = httpClient.GetAsync(apiUri).Result;
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
                    // put your own Bearer token here
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJhZG1pbiIsImV4cCI6MTc0NDU4MDIzNX0.MXX2ELDlDi7WYEYUiDhkNQ4D1BJzwkxTfBa1SOA-0vQ");
                    var userResult = httpClient.GetAsync($"user/{ev.CreatedBy}").Result;
                    if (userResult.IsSuccessStatusCode)
                    {
                        var users = userResult.Content.ReadAsAsync<List<UserViewModel>>().Result;
                        var user = users.Where(u => u.IdUser == ev.CreatedBy).FirstOrDefault();
                        if (user != null)
                        {
                            ev.CreatedByNavigation = new UserViewModel
                            {
                                Username = user.Username
                            };
                        }
                        else
                        {
                            _logger.LogError($"User with ID {ev.CreatedBy} not found in the response.");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Failed to retrieve user with ID {ev.CreatedBy}");
                    }
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
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            HttpResponseMessage response = httpClient.GetAsync($"{apiUri}/{id}").Result;
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
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            using var response = await httpClient.PostAsync(apiUri, postEvent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: EventController/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            HttpResponseMessage response = httpClient.GetAsync($"{apiUri}/{id}").Result;
            if (response.IsSuccessStatusCode)
            {
                var @event = response.Content.ReadAsAsync<EventViewModel>().Result;
                return View(@event);
            }
            else return NotFound();
        }

        // POST: EventController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, EventViewModel viewModel, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var existingEventResponse = httpClient.GetAsync($"{apiUri}/{id}").Result;
            var existingEvent = existingEventResponse.Content.ReadAsAsync<EventViewModel>().Result;

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

            viewModel.UpdatedAt = DateTime.UtcNow;
            viewModel.EventDate = viewModel.EventDate.Date;

            var putEvent = new StringContent(
                JsonSerializer.Serialize(viewModel),
                Encoding.UTF8,
                Application.Json);


            using var response = httpClient.PutAsync($"{apiUri}/{viewModel.IdEvent}", putEvent).Result;

            // Alt solution if the above doesn't work
            //using var response = httpClient.PutAsJsonAsync($"events/{viewModel.IdEvent}", viewModel).Result;

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError(string.Empty, errorContent);
                return View(viewModel);
            }
            else
            {
                return View(viewModel);
            }
        }

        // GET: EventController/Delete/5
        public IActionResult Delete2(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            HttpResponseMessage response = httpClient.GetAsync($"{apiUri}/{id}").Result;
            if (response.IsSuccessStatusCode)
            {
                var @event = response.Content.ReadAsAsync<EventViewModel>().Result;
                return View(@event);
            }
            else return NotFound();
        }

        // POST: EventController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int id, EventViewModel eventDTO)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var response = httpClient.DeleteAsync($"{apiUri}/{id}").Result;
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else return NotFound();
        }
    }
}
