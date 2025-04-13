using meeplematch_web.DTO;
using meeplematch_web.Interfaces;

namespace meeplematch_web.Service
{
    public class EventApiService : IEventApiService
    {
        private readonly HttpClient _http;

        public EventApiService(HttpClient httpClient)
        {
            _http = httpClient;
            _http.BaseAddress = new Uri("https://localhost:7230"); 
        }
        public async Task CreateAsync(EventDTO dto)
        {
            var response = await _http.PostAsJsonAsync("/api/meeplematch/events", dto);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"/api/meeplematch/events/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<EventDTO>> GetAllAsync()
        {
            var response = await _http.GetFromJsonAsync<List<EventDTO>>("/api/meeplematch/events");
            return response ?? new List<EventDTO>();
        }

        public async Task<EventDTO?> GetByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<EventDTO>($"/api/meeplematch/events/{id}");
        }

        public async Task UpdateAsync(int id, EventDTO dto)
        {
            var response = await _http.PutAsJsonAsync($"/api/meeplematch/events?id={id}", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
