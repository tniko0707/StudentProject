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
        /// <summary>
        /// Создать событие
        /// </summary>
        /// <param name="createEventDto"></param>
        /// <returns></returns>
        public void CreateEvent(CreateEventDto createEventDto)
        {
            Event evente = new Event()
            {
                Id = events.Select(e => e.Id).Max() + 1,
                Title = createEventDto.Title,
                Description = createEventDto.Description,
                StartAt = createEventDto.StartAt,
                EndAt = createEventDto.EndAt,
            };
            events.Add(evente);
        }
        /// <summary>
        /// Удаление события по id
        /// </summary>
        /// <param name="id"></param>
        public void DeleteEvent(int id)
        {
            Event? evente = GetEventById(id);
            if (evente != null) events.Remove(evente);
        }
        /// <summary>
        /// Получить все события
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Event> GetAllEvents()
        {
            return events.Cast<Event>();
        }
        /// <summary>
        /// Получить событие по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Event? GetEventById(int id)
        {
            return events.FirstOrDefault(e => e.Id == id) as Event;
        }
        /// <summary>
        /// Обновить событие
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateEventDto"></param>
        public void UpdateEvent(int id, UpdateEventDto updateEventDto)
        {
            Event? eventToUpdate = GetEventById(id);
            if (eventToUpdate != null)
            {
                int index = events.IndexOf(eventToUpdate);
                events[index] = new Event()
                {
                    Id = id,
                    Title = updateEventDto.Title,
                    Description = updateEventDto.Description,
                    StartAt = updateEventDto.StartAt,
                    EndAt = updateEventDto.EndAt,
                }; ;
            }
        }
        /// <summary>
        /// Получить последнее событие
        /// </summary>
        /// <returns></returns>
        public Event GetLastEvent()
        {
            return events.Last();
        }
    }
}
