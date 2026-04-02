namespace Project.Models
{
    public interface IEventService
    {
        IEnumerable<Event> GetAllEvents();
        Event? GetEventById(int id);
        void CreateEvent(CreateEventDto createEventDto);
        void UpdateEvent(int id, UpdateEventDto updateEventDto);
        void DeleteEvent(int id);
        Event GetLastEvent();
        PaginatedResult GetFilteredEvents(
            string title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 2);

    }
}
