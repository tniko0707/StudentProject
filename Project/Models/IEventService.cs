namespace Project.Models
{
    public interface IEventService
    {
        IEnumerable<Event> GetAllEvents();
        Event? GetEventById(int id);
        void CreateEvent(Event evente);
        void UpdateEvent(int id, Event evente);
        void DeleteEvent(int id);
    }
}
