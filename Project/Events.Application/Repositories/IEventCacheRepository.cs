using Events.Application.DTO;
using Events.Domain.Models;

namespace Events.Application.Repositories
{
    public interface IEventCacheRepository
    {
        Task DeleteKey(Guid id);
        Task<EventResponseDTO?> GetEventByIdAsync(Guid id, CancellationToken ct=default);
        Task<List<EventResponseDTO>> GetTop10Events(CancellationToken ct=default);
    }
}
