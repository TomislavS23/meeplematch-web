using meeplematch_web.DTO;
using meeplematch_web.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace meeplematch_web.Service
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        public AuthService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://localhost:7230");
        }
        public async Task<string?> LoginAsync(string username, string password)
        {
            var response = await _http.GetAsync($"api/meeplematch/auth/login?username={username}&password={password}");
            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        public async Task<string?> RegisterAsync(RegisterDTO register)
        {
            var json = JsonSerializer.Serialize(register);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("api/meeplematch/auth/register", content);
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadAsStringAsync();
        }
    }
}
