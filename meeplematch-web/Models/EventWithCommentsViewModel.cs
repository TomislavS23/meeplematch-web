namespace meeplematch_web.Models
{
    public class EventWithCommentsViewModel
    {
        public EventViewModel Event { get; set; }
        public List<EventCommentViewModel> Comments { get; set; }
        public EventCommentViewModel NewComment { get; set; } = new EventCommentViewModel();
    }
}
