using meeplematch_web.DTO;

namespace meeplematch_web.Interfaces
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string username, string password);
        Task<string?> RegisterAsync(RegisterDTO register);
    }
}
