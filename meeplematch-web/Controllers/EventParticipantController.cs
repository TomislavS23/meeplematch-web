using meeplematch_web.DTO;
using meeplematch_web.Models;
using meeplematch_web.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace meeplematch_web.Controllers
{
    public class EventParticipantController : Controller
    {
        private readonly ILogger<EventParticipantController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string apiUri = "event-participant";
        public EventParticipantController(ILogger<EventParticipantController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // GET: EventParticipantController
        public async Task<IActionResult> Index(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(Constants.ApiName);
                var response = await client.GetAsync($"{apiUri}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var eventParticipants = await response.Content.ReadAsAsync<List<EventParticipantViewModel>>();
                    return View(eventParticipants);
                }
                else
                {
                    TempData["toast_error"] = "Failed to load event participants!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load event participants");
                TempData["toast_error"] = "An error occurred while loading event participants.";
            }
            return View();
        }

        // GET: EventParticipantController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EventParticipantController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        // POST: EventParticipantController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(Constants.ApiName);
                var user = await (await client.GetAsync($"user/public/{User.Identity.Name}")).Content.ReadAsAsync<PublicUserDTO>();
                var participant = new EventParticipantViewModel
                {
                    IdUser = user.IdUser,
                    IdEvent = eventId,
                    IsJoined = true,
                    Username = user.Username,
                    JoinedAt = DateTime.UtcNow
                };
                var content = new StringContent(JsonSerializer.Serialize(participant), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiUri}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["toast_success"] = "Successfully joined the event!";
                }
                else
                {
                    TempData["toast_error"] = "Failed to join the event!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join the event");
                TempData["toast_error"] = "An error occurred while joining the event.";
            }

            return RedirectToAction(nameof(Details), "Event", new { id = eventId });
        }

        // GET: EventParticipantController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EventParticipantController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EventParticipantController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EventParticipantController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult DeleteUserFromEvent(int id)
        {
            return View();
        }

        // POST: EventParticipantController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserFromEvent(int eventId, int userId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient(Constants.ApiName);
                var response = await client.DeleteAsync($"{apiUri}?eventId={eventId}&userId={userId}");
                return RedirectToAction(nameof(Details), "Event", new { id = eventId });
            }
            catch
            {
                return View();
            }
        }
    }
}
