using meeplematch_api.Model;
using System.ComponentModel.DataAnnotations;

namespace meeplematch_web.Models
{
    public class EventViewModel
    {
        public int IdEvent { get; set; }

        public Guid? Uuid { get; set; }

        public string Name { get; set; } = null!;

        public string Type { get; set; } = null!;

        public string Game { get; set; } = null!;

        public string Location { get; set; } = null!;

        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        public int? Capacity { get; set; }

        [Display(Name = "Min Participants")]
        public int? MinParticipants { get; set; }

        [Display(Name = "Organized By")]
        public int CreatedBy { get; set; }

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        public virtual User CreatedByNavigation { get; set; } = null!;

        public virtual ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();

        public virtual ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();

        public virtual ICollection<EventRating> EventRatings { get; set; } = new List<EventRating>();

        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
