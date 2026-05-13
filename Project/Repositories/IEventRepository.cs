using Project.Models;

namespace Project.Repositories
{
    public interface IEventRepository
    {
        Task AddAsync(Event evente, CancellationToken ct = default);
        Task<Event?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Event>> GetAll(CancellationToken ct = default);
        Task<Event?> GetLast(CancellationToken ct);
        IQueryable<Event> GetQuery();
        Task RemoveEvent(Event evente, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct);
    }
}