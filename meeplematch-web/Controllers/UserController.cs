using AutoMapper;
using meeplematch_web.DTO;
using meeplematch_web.Models;
using meeplematch_web.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace meeplematch_web.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly string apiUrl = "user";

        public UserController(IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult StoreToken([FromBody] string jwt)
        {
            HttpContext.Session.SetString(Constants.JwtTokenFromSession, jwt);
            return Ok();
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel viewModel, IFormFile? image, string JwtToken)
        {
            if (!ModelState.IsValid)
            {
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

            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            if (!string.IsNullOrEmpty(JwtToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
            }

            var content = new StringContent(
                JsonSerializer.Serialize(viewModel),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync("user", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);

            var jwt = HttpContext.Session.GetString(Constants.JwtTokenFromSession);
            if (string.IsNullOrEmpty(jwt))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = "/User/Edit/" + id });
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await httpClient.GetAsync($"{apiUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<CreateUserViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ViewBag.JwtToken = jwt;
            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateUserViewModel viewModel, IFormFile? image, string JwtToken)
        {
            if (!ModelState.IsValid)
            {
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

            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            if (!string.IsNullOrEmpty(JwtToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
            }

            var content = new StringContent(JsonSerializer.Serialize(viewModel), Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"{apiUrl}/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var jwt = HttpContext.Session.GetString(Constants.JwtTokenFromSession);

            if (string.IsNullOrEmpty(jwt))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = $"/User/Details/{id}" });
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await httpClient.GetAsync($"{apiUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<CreateUserViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var jwt = HttpContext.Session.GetString(Constants.JwtTokenFromSession);

            if (string.IsNullOrEmpty(jwt))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = $"/User/Delete/{id}" });
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var response = await httpClient.GetAsync($"{apiUrl}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<CreateUserViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ViewBag.JwtToken = jwt;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string JwtToken)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);

            if (!string.IsNullOrEmpty(JwtToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
            }

            var response = await httpClient.DeleteAsync($"{apiUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var jwt = HttpContext.Session.GetString(Constants.JwtTokenFromSession);

            if (string.IsNullOrEmpty(jwt))
                return RedirectToAction("Login", "Auth", new { returnUrl = "/User/Profile" });

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var publicResponse = await httpClient.GetAsync($"user/public/{username}");
            if (!publicResponse.IsSuccessStatusCode)
                return NotFound();

            var publicUser = await publicResponse.Content.ReadFromJsonAsync<PublicUserViewModel>();

            var fullResponse = await httpClient.GetAsync($"user/{publicUser.IdUser}");
            if (!fullResponse.IsSuccessStatusCode)
                return NotFound();

            var user = await fullResponse.Content.ReadFromJsonAsync<CreateUserViewModel>();

            ViewBag.JwtToken = jwt;
            return View("Edit", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(CreateUserViewModel viewModel, IFormFile? image, string JwtToken)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", viewModel);
            }

            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                viewModel.ImagePath = $"/assets/pic_uploads/{uniqueFileName}";
            }

            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            if (!string.IsNullOrEmpty(JwtToken))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
            }

            var content = new StringContent(JsonSerializer.Serialize(viewModel), Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync("user/me", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["toast_success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError(string.Empty, $"API Error: {error}");
            return View("Edit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetMe()
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var jwt = HttpContext.Session.GetString(Constants.JwtTokenFromSession);

            if (string.IsNullOrEmpty(jwt))
            {
                TempData["toast_error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Auth");
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) { 
                return Unauthorized();
            }

            var publicResponse = await httpClient.GetAsync($"user/public/{username}");
            if (!publicResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }

            var publicUser = await publicResponse.Content.ReadFromJsonAsync<PublicUserViewModel>();

            var forgetResponse = await httpClient.PutAsync($"user/forget/{publicUser.IdUser}", null);
            if (!forgetResponse.IsSuccessStatusCode)
            {
                TempData["toast_error"] = "Failed to delete your account. Please try again.";
                return RedirectToAction("Profile");
            }

            await HttpContext.SignOutAsync("MyCookieAuth");
            HttpContext.Session.Clear();

            TempData["toast_success"] = "Your account has been deleted.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var jwt = HttpContext.Session.GetString(Constants.JwtTokenFromSession);

            if (string.IsNullOrEmpty(jwt))
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = "/User/MyEvents" });
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var publicResponse = await httpClient.GetAsync($"user/public/{username}");
            if (!publicResponse.IsSuccessStatusCode)
            {
                return NotFound("User not found");
            }

            var publicUser = await publicResponse.Content.ReadFromJsonAsync<PublicUserViewModel>();

            var createdEventsResponse = await httpClient.GetAsync($"events/created_by/{publicUser.IdUser}");
            var createdEvents = new List<EventViewModel>();
            if (createdEventsResponse.IsSuccessStatusCode)
            {
                createdEvents = await createdEventsResponse.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? new();
                await EnrichEventsAsync(httpClient, createdEvents);
            }

            var participantResponse = await httpClient.GetAsync($"event-participant/get_by_user/{publicUser.IdUser}");
            var eventParticipantDTOs = new List<EventParticipantViewModel>();
            if (participantResponse.IsSuccessStatusCode)
            {
                eventParticipantDTOs = await participantResponse.Content.ReadFromJsonAsync<List<EventParticipantViewModel>>() ?? new();
            }

            var registeredEvents = new List<EventViewModel>();
            var allEventsResponse = await httpClient.GetAsync("events");
            if (allEventsResponse.IsSuccessStatusCode)
            {
                var allEvents = await allEventsResponse.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? new();
                var eventIds = eventParticipantDTOs.Select(p => p.IdEvent).Distinct().ToList();
                registeredEvents = allEvents.Where(e => eventIds.Contains(e.IdEvent)).ToList();
                await EnrichEventsAsync(httpClient, registeredEvents);
            }

            var model = new UserEventsViewModel
            {
                CreatedEvents = createdEvents,
                RegisteredEvents = registeredEvents
            };

            return View(model); 
        }

        private async Task EnrichEventsAsync(HttpClient httpClient, List<EventViewModel> events)
        {
            foreach (var ev in events)
            {
                var participants = httpClient.GetAsync($"event-participant/{ev.IdEvent}").Result.Content.ReadAsAsync<List<EventParticipantViewModel>>().Result;
                ev.NumberOfParticipants = participants.Count();

                if (ev.CreatedBy != 0)
                {
                    HttpResponseMessage userResponse = await httpClient.GetAsync($"user/public/{ev.CreatedBy}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var user = await userResponse.Content.ReadAsAsync<PublicUserViewModel>();
                        ev.CreatedByNavigation = _mapper.Map<PublicUserViewModel>(user ?? new PublicUserViewModel { Username = "Unknown" });
                    }
                }
            }
        }
    }
}
