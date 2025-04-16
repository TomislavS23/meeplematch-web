////﻿using Microsoft.AspNetCore.Http;
////using Microsoft.AspNetCore.Mvc;
////using meeplematch_web.DTO;
////using AutoMapper;
////using meeplematch_web.Models;
////using meeplematch_web.Interfaces;

//﻿using Microsoft.AspNetCore.Mvc;
//using AutoMapper;
//using meeplematch_web.Models;
//using System.Text.Json;
//using System.Text;
//using static System.Net.Mime.MediaTypeNames;

//namespace meeplematch_web.Controllers
//{
//    //[Route("Event/[action]/{id?}")]
//    public class EventController : Controller
//    {
//        private readonly ILogger<EventController> _logger;
//        //private readonly IEventApiService _eventService;
//        //private readonly IUserApiService _userService;
//        private readonly IMapper _mapper;
//        private readonly IHttpClientFactory _httpClientFactory;

//        //public EventController(ILogger<EventController> logger, IEventApiService eventService, 
//        //    IUserApiService userService, IMapper mapper)
//        public EventController(ILogger<EventController> logger, IMapper mapper, IHttpClientFactory httpClientFactory)
//        {
//            _logger = logger;
//            _mapper = mapper;
//            _httpClientFactory = httpClientFactory;
//        }

//<<<<<<< HEAD
//        public async Task<IActionResult> Index(string search = "", int page = 1, int pageSize = 12)
//        {
//            var allEvents = await _eventService.GetAllAsync();

//            if (!string.IsNullOrEmpty(search))
//            {
//                allEvents = allEvents
//                    .Where(e => e.Name.ToLower().Contains(search.ToLower()) || e.Game.ToLower().Contains(search.ToLower()))
//                    .ToList();
//            }

//            var pagedEvents = allEvents
//                .OrderBy(e => e.EventDate)
//                .Skip((page - 1) * pageSize)
//                .Take(pageSize)
//                .ToList();

//            var viewModels = _mapper.Map<List<EventViewModel>>(pagedEvents);

//            foreach (var ev in viewModels)
//            {
//                var publicUser = await _userService.GetPublicUserAsync(ev.CreatedBy);
//                ev.CreatedByNavigation = new UserViewModel { Username = publicUser?.Username ?? "Unknown" };
//            }

//            ViewBag.CurrentPage = page;
//            ViewBag.TotalPages = (int)Math.Ceiling((double)allEvents.Count / pageSize);

//            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                return PartialView("_EventListPartial", viewModels);

//            return View(viewModels);
//=======
//        public IActionResult Index(string search = "", int page = 1, int pageSize = 3)
//        {
//            var httpClient = _httpClientFactory.CreateClient("MeepleMatch");
//                HttpResponseMessage response = httpClient.GetAsync("events").Result;
//                if (response.IsSuccessStatusCode)
//                {
//                    var events = string.IsNullOrEmpty(search)
//                        ? response.Content.ReadAsAsync<List<EventViewModel>>().Result
//                        : response.Content.ReadAsAsync<List<EventViewModel>>().Result
//                            .Where(e => e.Name.ToLower().Contains(search.ToLower()) || e.Game.ToLower().Contains(search.ToLower()))
//                            .ToList();

//                    var pagedEvents = events
//                        .OrderBy(e => e.EventDate)
//                        .Skip((page - 1) * pageSize)
//                        .Take(pageSize)
//                        .ToList();

//                    foreach (var ev in pagedEvents)
//                    {
//                        var userDto = _userRepository.GetUser(ev.CreatedBy);
//                        ev.CreatedByNavigation = new User
//                        {
//                            Username = userDto.Username
//                        };
//                    }

//                    ViewBag.CurrentPage = page;
//                    ViewBag.TotalPages = (int)Math.Ceiling((double)events.Count / pageSize);

//                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
//                    {
//                        return PartialView("_EventListPartial", pagedEvents);
//                    }
//                    return View(pagedEvents);
//                }

//            return StatusCode(500);
//>>>>>>> dd068ca (Partial rewrite of the Event controller)
//        }

//        // GET: EventController/Details/5
//        public async Task<IActionResult> Details(int id)
//        {
//<<<<<<< HEAD
//            var dto = await _eventService.GetByIdAsync(id);
//            if (dto == null) return NotFound();

//            var viewModel = _mapper.Map<EventViewModel>(dto);
//            var publicUser = await _userService.GetPublicUserAsync(viewModel.CreatedBy);
//            viewModel.CreatedByNavigation = new UserViewModel { Username = publicUser?.Username ?? "Unknown" };

//            return View(viewModel);
//=======
//            var httpClient = _httpClientFactory.CreateClient("MeepleMatch");
//            HttpResponseMessage response = httpClient.GetAsync($"events/{id}").Result;
//                if (response.IsSuccessStatusCode)
//                {
//                    var @event = response.Content.ReadAsAsync<EventViewModel>().Result;
//                    return View(@event);
//                }

//            return NotFound();
//>>>>>>> dd068ca (Partial rewrite of the Event controller)
//        }

//        // GET: EventController/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: EventController/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(EventViewModel viewModel, IFormFile? image)
//        {

//            if (!ModelState.IsValid)
//            {
//                return View(viewModel);
//            }

//            if (image != null && image.Length > 0)
//            {
//                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
//                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads", fileName);
//                using (var stream = new FileStream(filePath, FileMode.Create))
//                    await image.CopyToAsync(stream);

//                viewModel.ImagePath = $"/assets/pic_uploads/{fileName}";
//            }

//            viewModel.CreatedBy = 1;
//            viewModel.CreatedAt = DateTime.UtcNow;

//            var postEvent = new StringContent(
//                JsonSerializer.Serialize(viewModel),
//                Encoding.UTF8,
//                Application.Json);
//            var httpClient = _httpClientFactory.CreateClient("MeepleMatch");
//            using var response = await httpClient.PostAsync("events", postEvent);

//            if (response.IsSuccessStatusCode)
//            {
//                return RedirectToAction(nameof(Index));
//            }

//            return View(viewModel);
//        }

//        // GET: EventController/Edit/5
//        public async Task<IActionResult> Edit(int id)
//        {
//            var dto = await _eventService.GetByIdAsync(id);
//            if (dto == null) return NotFound();

//            var viewModel = _mapper.Map<EventViewModel>(dto);
//            return View(viewModel);
//        }

//        // POST: EventController/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, EventViewModel viewModel, IFormFile image)
//        {
//            if (!ModelState.IsValid) return View(viewModel);

//            if (image != null && image.Length > 0)
//            {
//                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
//                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "pic_uploads", fileName);
//                using (var stream = new FileStream(filePath, FileMode.Create))
//                    await image.CopyToAsync(stream);

//                viewModel.ImagePath = $"/assets/pic_uploads/{fileName}";
//            }

//            var dto = _mapper.Map<EventDTO>(viewModel);
//            dto.UpdatedAt = DateTime.UtcNow;

//            await _eventService.UpdateAsync(id, dto);

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}

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

            // The following is a test
            // TODO: CreatedBy should be set to the logged-in user
            //var jwtToken = HttpContext.Session.GetString(Constants.JwtTokenFromSession);
            //var token = JwtUtils.ConvertJwtStringToJwtSecurityToken(jwtToken);
            //var payload = JwtUtils.DecodeJwt(token);

            //var username = payload.FirstOrDefault(x => x.Key.Contains("name")).Value.ToString();

            //Console.WriteLine(username);

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