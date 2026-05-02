using Project.Models;

namespace Project.Services
{
    public interface IEventService
    {
        IEnumerable<Event> GetAllEvents();
        Event? GetEventById(Guid id);
        void CreateEvent(CreateEventDto createEventDto);
        void UpdateEvent(Guid id, UpdateEventDto updateEventDto);
        void DeleteEvent(Guid id);
        Event GetLastEvent();
        PaginatedResult GetFilteredEvents(
            string title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 2);

    }
}
