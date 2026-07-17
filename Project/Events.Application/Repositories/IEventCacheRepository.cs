using Events.Domain.Models;

namespace Events.Application.Repositories
{
    public interface IEventCacheRepository
    {
        Task DeleteKey(Guid id);
        Task<Event?> GetEventByIdAsync(Guid id, CancellationToken ct=default);
        Task<List<Event>> GetTop10Events(CancellationToken ct=default);
    }
}
