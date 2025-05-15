using meeplematch_web.DTO;

namespace meeplematch_web.Models
{
    public class UserEventsViewModel
    {
        public List<EventViewModel> CreatedEvents { get; set; } = new();
        public List<EventViewModel> RegisteredEvents { get; set; } = new();
    }
}
