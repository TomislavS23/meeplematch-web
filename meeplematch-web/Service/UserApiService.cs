using meeplematch_web.DTO;
using meeplematch_web.Interfaces;
using meeplematch_web.Utils;

namespace meeplematch_web.Service
{
    public class UserApiService : IUserApiService
    {
        //private readonly HttpClient _http;
        //public UserApiService(HttpClient http)
        //{
        //    _http = http;
        //    _http.BaseAddress = new Uri("https://localhost:7230");
        //}
        private readonly IHttpClientFactory _httpClientFactory;

        public UserApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<UserDTO?> GetByIdAsync(int id)
        {
            var _http = _httpClientFactory.CreateClient(Constants.ApiName);
            return await _http.GetFromJsonAsync<UserDTO>($"/api/meeplematch/user/{id}");
        }

        public async Task<PublicUserDTO?> GetPublicUserAsync(int id)
        {
            var _http = _httpClientFactory.CreateClient(Constants.ApiName);
            return await _http.GetFromJsonAsync<PublicUserDTO>($"/api/meeplematch/user/public/{id}");
        }
    }
}
