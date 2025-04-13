using meeplematch_web.DTO;
using meeplematch_web.Interfaces;

namespace meeplematch_web.Service
{
    public class UserApiService : IUserApiService
    {
        private readonly HttpClient _http;
        public UserApiService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://localhost:7230");
        }
        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<UserDTO>($"/api/meeplematch/user/{id}");
        }

        public async Task<PublicUserDTO?> GetPublicUserAsync(int id)
        {
            return await _http.GetFromJsonAsync<PublicUserDTO>($"/api/meeplematch/user/public/{id}");
        }
    }
}
