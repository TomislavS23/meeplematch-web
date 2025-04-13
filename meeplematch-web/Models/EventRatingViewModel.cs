namespace meeplematch_web.Models
{
    public class EventRatingViewModel
    {
        public int IdEventRating { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public int Rating { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual EventViewModel Event { get; set; } = null!;

        public virtual UserViewModel User { get; set; } = null!;
    }
}
