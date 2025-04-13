namespace meeplematch_web.Models
{
    public class ReportViewModel
    {
        public int IdReport { get; set; }

        public int ReportedBy { get; set; }

        public int? EventId { get; set; }

        public string Reason { get; set; } = null!;

        public int Status { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual EventViewModel? Event { get; set; }

        public virtual UserViewModel ReportedByNavigation { get; set; } = null!;

        //public virtual ReportStatus StatusNavigation { get; set; } = null!;
    }
}
