
namespace Project.Models
{
    public class EventService : IEventService
    {
        private static readonly List<Event> events = new List<Event>()
        {
            new Event() 
            {
                Id = 1,
                Title="имя",
                Description="описание",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(1)
            },
            new Event()
            {
                Id = 3,
                Title="имя3",
                Description="описание",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(3)
            }
        };
        public void CreateEvent(Event evente)
        {
            events.Add(evente);
        }

        public void DeleteEvent(int id)
        {
            Event? evente = GetEventById(id);
            if (evente != null ) events.Remove(evente);
        }

        public IEnumerable<Event> GetAllEvents()
        {
            return events;
        }

        public Event? GetEventById(int id)
        {
            return events.FirstOrDefault(e => e.Id == id);
        }

        public void UpdateEvent(int id, Event evente)
        {
            Event? eventToUpdate = GetEventById(id);

            if (eventToUpdate != null)
            {
                int index = events.IndexOf(eventToUpdate);
                events[index] = evente;
            }
        }
    }
}
