using Events.Application.Repositories;
using Events.Domain.Models;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _db;
        public EventRepository(AppDbContext context) { _db = context; }
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _db.SaveChangesAsync(ct);
        }
        public async Task AddAsync(Event evente, CancellationToken ct = default)
        {
            await _db.Events.AddAsync(evente, ct);
            await SaveChangesAsync(ct);
        }
        public async Task<Event?> FindByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        }
        public async Task RemoveEvent(Event evente, CancellationToken ct = default)
        {
            _db.Events.Remove(evente);
            await SaveChangesAsync(ct);
        }

        public async Task<List<Event>> GetAll(CancellationToken ct = default)
        {
            return await _db.Events.ToListAsync(ct);
        }

        public async Task<Event?> GetLast(CancellationToken ct = default)
        {
            return await _db.Events.OrderBy(e => e.StartAt).LastOrDefaultAsync(ct);
        }

        public IQueryable<Event> GetQuery()
        {
            return _db.Events.AsQueryable();
        }

    }
}
