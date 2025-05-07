namespace meeplematch_web.Models
{
    public class EventParticipantViewModel
    {
        public int IdEventParticipant { get; set; }

        public int IdEvent { get; set; }

        public int IdUser { get; set; }

        public DateTime? JoinedAt { get; set; }

        public bool? IsJoined { get; set; }
        public string? Username { get; set; }

        public virtual EventViewModel IdEventNavigation { get; set; } = null!;

        public virtual UserViewModel IdUserNavigation { get; set; } = null!;
    }
}
