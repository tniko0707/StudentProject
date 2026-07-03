using Events.Application.Common;
using Events.Application.DTO;
using Events.Domain.Models;

namespace Events.Application.Services
{
    public interface IEventService
    {
        Task<IEnumerable<Event>> GetAllEventsAsync(CancellationToken cancellationToken);
        Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> CreateEventAsync(CreateEventDto createEventDto, CancellationToken cancellationToken);
        Task<Event?> UpdateEventAsync(Guid id, UpdateEventDto updateEventDto, CancellationToken cancellationToken);
        Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken);
        Task<Event> GetLastEventAsync(CancellationToken cancellationToken);
        Task<PaginatedResult> GetFilteredEventsAsync(
            string title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

    }
}
