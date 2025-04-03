using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using meeplematch_api.Model;
using meeplematch_api.Service;
using Microsoft.EntityFrameworkCore;
using meeplematch_api.Repository;
using meeplematch_api.DTO;
using AutoMapper;
using meeplematch_web.Models;

namespace meeplematch_web.Controllers
{
    //[Route("Event/[action]/{id?}")]
    public class EventController : Controller
    {
        private readonly ILogger<EventController> _logger;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventController(ILogger<EventController> logger, IEventRepository eventRepository, IMapper mapper)
        {
            _logger = logger;
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public IActionResult Index(string search = "")
        {
            var events = string.IsNullOrEmpty(search) ? _eventRepository.FindAll() : _eventRepository.FindAll()
                .Where(e => e.Name.ToLower().Contains(search.ToLower()) || e.Game.ToLower().Contains(search.ToLower()));
            var eventsViewModel = _mapper.Map<IEnumerable<EventViewModel>>(events);

            if(Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_EventListPartial", eventsViewModel);
            }

            return View(eventsViewModel);
        }

        // GET: EventController/Details/5
        public IActionResult Details(int id)
        {
            var @event = _eventRepository.FindById(id);
            if (@event is null) return NotFound();
            var eventViewModel = _mapper.Map<EventViewModel>(@event);
            return View(eventViewModel);
        }

        // GET: EventController/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventDTO @event)
        {
            if (ModelState.IsValid)
            {
                @event.CreatedAt = DateTime.UtcNow;
                _eventRepository.Save(@event);
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogError(error.ErrorMessage);
            }

            var eventViewModel = _mapper.Map<EventViewModel>(@event);
            return View(eventViewModel);
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
        public IActionResult Edit(int id, EventDTO @event)
        {
            if (@event is null) return NotFound();

            if (ModelState.IsValid)
            {
                @event.UpdatedAt = DateTime.UtcNow;
                _eventRepository.Update(@event, id);
                return RedirectToAction(nameof(Index));
            }

            var eventViewModel = _mapper.Map<EventViewModel>(@event);
            return View(eventViewModel);
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
        //[ValidateAntiForgeryToken]
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
    }
}
