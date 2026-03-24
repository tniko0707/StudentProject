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
    }
}
