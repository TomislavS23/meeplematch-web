namespace meeplematch_web.Models
{
    public class UserViewModel
    {
        public int IdUser { get; set; }
        public string Username { get; set; }

        public string Email { get; set; }

        public int RoleId { get; set; }

        public bool? IsBanned { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
