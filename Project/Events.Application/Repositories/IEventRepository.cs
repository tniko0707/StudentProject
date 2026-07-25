using Events.Domain.Models;

namespace Events.Application.Repositories
{
    public interface IEventRepository
    {
        Task AddAsync(Event evente, CancellationToken ct = default);
        Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Event>> GetTop10Async(CancellationToken ct = default);
        Task UpdateAsync(Event updatedEvente, CancellationToken ct = default);
        Task<List<Event>> GetAllAsync(CancellationToken ct = default);
        Task<Event?> GetLastAsync(CancellationToken ct);
        Task DeleteAsync(Event evente, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct);
        IQueryable<Event> GetQuery();
    }
}