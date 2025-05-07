namespace meeplematch_web.Models
{
    public class EventWithCommentsAndParticipantsViewModel
    {
        public EventViewModel Event { get; set; }
        public List<EventCommentViewModel> Comments { get; set; }
        public List<EventParticipantViewModel> Participants { get; set; }
        public EventCommentViewModel NewComment { get; set; } = new EventCommentViewModel();
    }
}
