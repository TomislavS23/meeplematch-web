using meeplematch_web.DTO;
using static System.Net.WebRequestMethods;

namespace meeplematch_web.Interfaces
{
    public interface IUserApiService
    {
        Task<UserDTO?> GetByIdAsync(int id);
        Task<PublicUserDTO?> GetPublicUserAsync(int id);
    }
}
