using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using meeplematch_web.Models;
using System.Text.Json;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using meeplematch_web.Utils;
using System.Net.Http;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using QRCoder;
using Microsoft.AspNetCore.Authorization;

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
                    var participants = httpClient.GetAsync($"event-participant/{ev.IdEvent}").Result.Content.ReadAsAsync<List<EventParticipantViewModel>>().Result;
                    ev.NumberOfParticipants = participants.Count();

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
        public async Task<IActionResult> Details(int id, int page = 1, int pageSize = 5)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            HttpResponseMessage response = await httpClient.GetAsync($"{apiUri}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var @event = await response.Content.ReadAsAsync<EventViewModel>();
                if (@event.CreatedBy != 0)
                {
                    HttpResponseMessage userResponse = await httpClient.GetAsync($"user/public/{@event.CreatedBy}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var user = await userResponse.Content.ReadAsAsync<PublicUserViewModel>();
                        @event.CreatedByNavigation = _mapper.Map<PublicUserViewModel>(user ?? new PublicUserViewModel { Username = "Unknown" });
                    }
                }

                var commentsResponse = await httpClient.GetAsync($"event-comment/{id}");
                List<EventCommentViewModel> comments = new();
                List<EventCommentViewModel> pagedComments = new();
                if (commentsResponse.IsSuccessStatusCode)
                {
                    comments = await commentsResponse.Content.ReadAsAsync<List<EventCommentViewModel>>();
                    foreach (var comment in comments)
                    {
                        HttpResponseMessage userResponse = await httpClient.GetAsync($"user/public/{comment.UserId}");
                        if (userResponse.IsSuccessStatusCode)
                        {
                            var user = await userResponse.Content.ReadAsAsync<PublicUserViewModel>();
                            comment.User = _mapper.Map<PublicUserViewModel>(user ?? new PublicUserViewModel { Username = "Unknown" });
                        }
                        comment.Event = @event;
                    }

                    pagedComments = comments.OrderByDescending(c => c.UpdatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    ViewData["CurrentPage"] = page;
                    ViewData["TotalPages"] = (int)Math.Ceiling((double)comments.Count / pageSize);
                }

                var participants = await (await httpClient.GetAsync($"event-participant/{id}")).Content.ReadAsAsync<List<EventParticipantViewModel>>();
                if (User.Identity.IsAuthenticated)
                {

                    foreach (var ev in participants)
                    {
                        //var user = await (await httpClient.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserViewModel>();
                        var user = await (await httpClient.GetAsync($"user/public/{ev.IdUser}")).Content.ReadAsAsync<PublicUserViewModel>();
                        ev.Username = user.Username;

                        if (ev.IdUser == user.IdUser) ev.IsJoined = true;
                        else ev.IsJoined = false;
                    }
                }

                var viewModel = new EventWithCommentsAndParticipantsViewModel
                {
                    Event = @event,
                    Comments = pagedComments,
                    Participants = participants,
                    NewComment = new EventCommentViewModel
                    {
                        EventId = id
                    }
                };

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return Ok(viewModel);
                }

                return View(viewModel);
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
        [Authorize]
        [HttpPost]
        //[ValidateAntiForgeryToken]
#if !TESTING
[ValidateAntiForgeryToken]
#endif

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

            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);

            var userResult = await httpClient.GetAsync($"user/public/{username}");
            var user = await userResult.Content.ReadAsAsync<PublicUserViewModel>();

            if (user == null)
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return BadRequest("User not found");
                }
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
        public async Task<IActionResult> Edit(int id, EventViewModel viewModel, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                TempData["toast_error"] = "Please fill in all required fields.";
                return View(viewModel);
            }

            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var existingEventResponse = await httpClient.GetAsync($"{apiUri}/{id}");
            var existingEvent = await existingEventResponse.Content.ReadAsAsync<EventViewModel>();

            var userEditing = await (await httpClient.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserViewModel>();

            if (userEditing.IdUser != existingEvent.CreatedBy && !User.IsInRole("Admin"))
            {
                TempData["toast_error"] = "An error occured while validating the user.";
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return BadRequest("Current user does not equal the original user");
                }
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction(nameof(Details), new { id = id });
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
        public async Task<IActionResult> Delete2(int id, EventViewModel eventDTO)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);

            var userDeleting = await (await httpClient.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserViewModel>();
            if (userDeleting.IdUser != eventDTO.CreatedBy && !User.IsInRole("Admin"))
            {
                TempData["toast_error"] = "An error occured while validating the user.";
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return BadRequest("Current user does not equal the original user");
                }
                return RedirectToAction(nameof(Index));
            }

            var response = await httpClient.DeleteAsync($"{apiUri}/{id}");
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

        public async Task<IActionResult> GenerateQrCode(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            HttpResponseMessage response = await httpClient.GetAsync($"{apiUri}/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["toast_error"] = "Event not found";
                return NotFound();
            }
            var @event = await response.Content.ReadAsAsync<EventViewModel>();
            var url = Url.Action("Details", "Event", new { id = id }, Request.Scheme);
            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var bitmap = qrCode.GetGraphic(20);

            var qrCodeImageBase64 = Convert.ToBase64String(bitmap);
            var qrCodeImageUrl = $"data:image/png;base64,{qrCodeImageBase64}";

            ViewBag.QrCodeImageUrl = qrCodeImageUrl;
            ViewBag.EventName = @event.Name;
            return View();
        }
    }
}