using meeplematch_web.Models;
using meeplematch_web.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace meeplematch_web.Controllers
{
    public class EventCommentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EventCommentController> _logger;
        private readonly string apiUrl = "event-comment";
        public EventCommentController(IHttpClientFactory httpClientFactory, ILogger<EventCommentController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: EventCommentController
        public async Task<IActionResult> Index(int id)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var response = await httpClient.GetAsync($"{apiUrl}/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var comments = JsonSerializer.Deserialize<List<EventCommentViewModel>>(content);
                return View(comments);
            }
            else
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return StatusCode(500, "Error fetching all comments.");
                }
                ViewData["Error"] = "Error fetching all comments";
                TempData["toast_error"] = "Error fetching all comments";
                return View();
            }
        }

        // GET: EventCommentController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        // GET: EventCommentController/Create
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: EventCommentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventCommentViewModel eventComment)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return Unauthorized("User not authenticated");
                    }
                    ViewData["Error"] = "User not authenticated";
                    TempData["toast_error"] = "User not authenticated";
                    return View();
                }
                var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
                var user = await (await httpClient.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserViewModel>();

                eventComment.UserId = user.IdUser;
                eventComment.CreatedAt = DateTime.UtcNow;

                var jsonContent = JsonSerializer.Serialize(eventComment);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(EventController.Details), "Event", new { id = eventComment.EventId });
                }
                else
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return StatusCode(500, "Error creating comment.");
                    }
                    ViewData["Error"] = "Error creating comment";
                    TempData["toast_error"] = "Error creating comment";
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: EventCommentController/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int eventId, int eventCommentId)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var response = await httpClient.GetAsync($"{apiUrl}/{eventId}");

            if (!response.IsSuccessStatusCode)
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return NotFound("Event not found.");
                }
                ViewData["Error"] = "Event not found";
                TempData["toast_error"] = "Event not found";
                return View();
            }

            var comments = await response.Content.ReadAsAsync<List<EventCommentViewModel>>();
            var comment = comments.Where(c => c.IdEventComment == eventCommentId).FirstOrDefault();
            return View(comment);
        }

        // POST: EventCommentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventCommentViewModel eventComment)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
                var comment = (await (await httpClient.GetAsync($"{apiUrl}/{eventComment.EventId}")).Content.ReadAsAsync<List<EventCommentViewModel>>()).Where(c => c.IdEventComment == eventComment.IdEventComment).FirstOrDefault();
                if (comment is null)
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return NotFound("Comment not found");
                    }
                    ViewData["Error"] = "Comment not found";
                    TempData["toast_error"] = "Comment not found";
                    return View();
                }


                eventComment.UpdatedAt = DateTime.UtcNow;
                //eventComment.EventId = comment.EventId;
                eventComment.UserId = comment.UserId;
                eventComment.CreatedAt = comment.CreatedAt;

                var user = await (await httpClient.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserViewModel>();
                if (user.IdUser != eventComment.UserId)
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return Unauthorized("You are not authorized");
                    }
                    ViewData["Error"] = "You are not authorized";
                    TempData["toast_error"] = "You are not authorized";
                    return View();
                }

                eventComment.User = user;

                if (!User.Identity.IsAuthenticated || !User.Identity.Name.Equals(eventComment.User.Username))
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return Unauthorized("You are not authorized");
                    }
                    ViewData["Error"] = "You are not authorized";
                    TempData["toast_error"] = "You are not authorized";
                    return View();
                }

                var jsonContent = JsonSerializer.Serialize(eventComment);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"{apiUrl}?eventCommentId={eventComment.IdEventComment}", content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(EventController.Details), "Event", new { id = eventComment.EventId });
                }
                else
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return StatusCode(500, "Error updating comment.");
                    }
                    ViewData["Error"] = "Error updating comment";
                    TempData["toast_error"] = "Error updating comment";
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: EventCommentController/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int eventId, int eventCommentId)
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
            var response = await httpClient.GetAsync($"{apiUrl}/{eventId}");
            if (!response.IsSuccessStatusCode)
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return NotFound("Event not found.");
                }
                ViewData["Error"] = "Event not found";
                TempData["toast_error"] = "Event not found";
                return View();
            }
            var comments = await response.Content.ReadAsAsync<List<EventCommentViewModel>>();
            var comment = comments.Where(c => c.IdEventComment == eventCommentId).FirstOrDefault();
            if (comment != null) {
                var user = await (await httpClient.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserViewModel>();
                if (user.IdUser != comment.UserId)
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return Unauthorized("You are not authorized");
                    }
                    ViewData["Error"] = "You are not authorized";
                    TempData["toast_error"] = "You are not authorized";
                    return View();
                }
                comment.User = user;
            }
            else
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                {
                    return NotFound("Comment not found");
                }
                ViewData["Error"] = "Comment not found";
                TempData["toast_error"] = "Comment not found";
            }
            return View(comment);
        }

        // POST: EventCommentController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, EventCommentViewModel eventComment)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(Constants.ApiName);
                var response = await httpClient.DeleteAsync($"{apiUrl}?eventCommentId={eventComment.IdEventComment}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(EventController.Details), "Event", new { id = eventComment.EventId });
                }
                else
                {
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing")
                    {
                        return StatusCode(500, "Error deleting comment.");
                    }
                    ViewData["Error"] = "Error deleting comment";
                    TempData["toast_error"] = "Error deleting comment";
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }
    }
}
