namespace meeplematch_web.Models
{
    public class EventCommentViewModel
    {
        public int IdEventComment { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public string Comment { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual EventViewModel Event { get; set; } = null!;

        public virtual UserViewModel User { get; set; } = null!;
    }
}
