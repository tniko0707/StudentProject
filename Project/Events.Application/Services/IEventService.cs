using Events.Application.Common;
using Events.Application.DTO;
using Events.Domain.Models;

namespace Events.Application.Services
{
    public interface IEventService
    {
        Task<IEnumerable<EventResponseDTO>> GetAllAsync(CancellationToken cancellationToken);
        Task<EventResponseDTO?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IEnumerable<EventResponseDTO>> GetTop10EventsAsync (CancellationToken cancellationToken);
        Task<EventResponseDTO> CreateEventAsync(CreateEventDto createEventDto, CancellationToken cancellationToken);
        Task<EventResponseDTO?> UpdateEventAsync(Guid id, UpdateEventDto updateEventDto, CancellationToken cancellationToken);
        Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken);
        Task<EventResponseDTO> GetLastEventAsync(CancellationToken cancellationToken);
        Task<PaginatedResult> GetFilteredEventsAsync(
            string title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

    }
}
