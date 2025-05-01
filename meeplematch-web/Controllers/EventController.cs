using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using meeplematch_web.Models;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using meeplematch_web.Utils;

namespace meeplematch_web.Controllers
{
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

        public IActionResult Index(string search = "", int page = 1, int pageSize = 12)
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
                    // Fix required: no need for authentication to view organizer's name
                    //httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Constants.JwtToken);
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString(Constants.JwtTokenFromSession));
                    var userResult = httpClient.GetAsync($"user/public/{ev.CreatedBy}").Result;
                    if (userResult.IsSuccessStatusCode)
                    {
                        var user = userResult.Content.ReadAsAsync<PublicUserViewModel>().Result;
                        ev.CreatedByNavigation = _mapper.Map<PublicUserViewModel>(user ?? new PublicUserViewModel { Username = "Unknown" });
                    }
                    else
                    {
                        _logger.LogError($"Failed to retrieve user with ID {ev.CreatedBy}");
                    }
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)events.Count / pageSize);

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return Ok(pagedEvents);
                }

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
                if (@event.CreatedBy != 0)
                {
                    HttpResponseMessage userResponse = httpClient.GetAsync($"user/public/{@event.CreatedBy}").Result;
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var user = userResponse.Content.ReadAsAsync<PublicUserViewModel>().Result;
                        @event.CreatedByNavigation = _mapper.Map<PublicUserViewModel>(user ?? new PublicUserViewModel { Username = "Unknown" });
                    }
                }
                return View(@event);
            }
            TempData["toast_error"] = "Event not found";
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
                TempData["toast_error"] = "Please fill in all required fields.";
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

            if (!User.Identity.IsAuthenticated)
            {
                TempData["toast_error"] = "You must be logged in to create an event!";
                return View(viewModel);
            }

            var username = User.Identity.Name;

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

            // The following is a test
            // TODO: CreatedBy should be set to the logged-in user
            //var jwtToken = HttpContext.Session.GetString(Constants.JwtTokenFromSession);
            //var token = new JwtSecurityToken();
            //if (jwtToken is not null)
            //{
            //    token = JwtUtils.ConvertJwtStringToJwtSecurityToken(jwtToken);
            //}
            //else
            //{
            //    TempData["toast_error"] = "You are not authenticated!";
            //    return View(viewModel);
            //}
            //var payload = JwtUtils.DecodeJwt(token);

            //var username = payload.FirstOrDefault(x => x.Key.Contains("name")).Value.ToString();

            //Console.WriteLine(username);


            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var jwtToken = HttpContext.Session.GetString(Constants.JwtTokenFromSession);

            if (string.IsNullOrEmpty(jwtToken))
            {
                TempData["toast_error"] = "Authentication error!";
                _logger.LogError("JWT token is null or empty");
                return View(viewModel);
            }

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

            //if (httpClient.DefaultRequestHeaders.Authorization == null)
            //{
            //    TempData["toast_error"] = "You are not authenticated!";
            //    _logger.LogError("Authorization header is null");
            //}
            //else
            //{
            //    _logger.LogInformation($"Authorization header: {httpClient.DefaultRequestHeaders.Authorization}");
            //}



            var usersResult = await httpClient.GetAsync($"user/");
            if (!usersResult.IsSuccessStatusCode)
            {
                TempData["toast_error"] = "Failed to retrieve user information.";
                _logger.LogError("Failed to retrieve user information.");
                return View(viewModel);
            }

            var user = (await usersResult.Content.ReadAsAsync<List<PublicUserViewModel>>()).FirstOrDefault(u => u.Username.Equals(username));

            //viewModel.CreatedBy = 1;
            if (user == null)
            {
                TempData["toast_error"] = "User not found.";
                _logger.LogError("User not found.");
                return View(viewModel);
            }

            viewModel.CreatedBy = user.IdUser;
            viewModel.CreatedAt = DateTime.UtcNow;

            var postEvent = new StringContent(
                JsonSerializer.Serialize(viewModel),
                Encoding.UTF8,
                Application.Json);
            using var response = await httpClient.PostAsync(apiUri, postEvent);

            if (response.IsSuccessStatusCode)
            {
                TempData["toast_success"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["toast_error"] = "An error occurred while creating the event.";
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
                TempData["toast_error"] = "Please fill in all required fields.";
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
                TempData["toast_success"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                TempData["toast_error"] = "Event not found";
                return NotFound();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                TempData["toast_error"] = "Bad input";
                var errorContent = response.Content.ReadAsStringAsync().Result;
                ModelState.AddModelError(string.Empty, errorContent);
                return View(viewModel);
            }
            else
            {
                TempData["toast_error"] = "An error occurred while updating the event.";
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
                TempData["toast_success"] = "Event deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            else {
                TempData["toast_error"] = "Event not found";
                return NotFound();
            } 
        }
    }
}