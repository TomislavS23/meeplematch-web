using meeplematch_web.DTO;

namespace meeplematch_web.Interfaces
{
    public interface IEventApiService
    {
        Task<List<EventDTO>> GetAllAsync();
        Task<EventDTO?> GetByIdAsync(int id);
        Task CreateAsync(EventDTO dto);
        Task UpdateAsync(int id, EventDTO dto);
        Task DeleteAsync(int id);
    }
}
