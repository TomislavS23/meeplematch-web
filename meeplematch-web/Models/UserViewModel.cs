using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Composition;
using System.Data;
using System.Diagnostics.Tracing;

namespace meeplematch_web.Models
{
    public class UserViewModel
    {
        public int IdUser { get; set; }

        public Guid Uuid { get; set; }

        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public byte[] HashedPassword { get; set; } = null!;

        public byte[] Salt { get; set; } = null!;

        public int RoleId { get; set; }

        public bool? IsBanned { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? ImagePath { get; set; }

        public bool? IsMale { get; set; }

        [ValidateNever]
        public virtual ICollection<EventCommentViewModel> EventComments { get; set; } = new List<EventCommentViewModel>();

        public virtual ICollection<EventParticipantViewModel> EventParticipants { get; set; } = new List<EventParticipantViewModel>();

        public virtual ICollection<EventRatingViewModel> EventRatings { get; set; } = new List<EventRatingViewModel>();

        public virtual ICollection<EventViewModel> Events { get; set; } = new List<EventViewModel>();

        //public virtual ICollection<GlobalMessage> GlobalMessages { get; set; } = new List<GlobalMessage>();

        //public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<ReportViewModel> Reports { get; set; } = new List<ReportViewModel>();

        //public virtual Role Role { get; set; } = null!;

        //public virtual ICollection<Telemetry> Telemetries { get; set; } = new List<Telemetry>();

        //public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
