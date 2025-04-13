namespace meeplematch_web.DTO;

public class UserDTO
{
    public int IdUser { get; set; }

    public Guid Uuid { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public byte[] HashedPassword { get; set; }

    public byte[] Salt { get; set; }

    public int RoleId { get; set; }

    public bool? IsBanned { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}