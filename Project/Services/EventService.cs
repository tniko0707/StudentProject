using Project.Models;
using System.ComponentModel.DataAnnotations;

namespace Project.Services
{
    public class EventService : IEventService
    {
        private static readonly List<Event> events = new List<Event>()
        {
            new Event(
                "имя",
                "описание",
                DateTime.Now,
                DateTime.Now.AddDays(1),
                4
            ),
            new Event(
                "имя2",
                "описание2",
                DateTime.Now,
                DateTime.Now.AddDays(3),
                5
            ),
            new Event(
                "имя3",
                "описание3",
                DateTime.Now,
                DateTime.Now.AddDays(3),
                6
            )
        };
        /// <summary>
        /// Создать событие
        /// </summary>
        /// <param name="createEventDto"></param>
        /// <returns></returns>
        public void CreateEvent(CreateEventDto createEventDto)
        {
            if (createEventDto.StartAt > createEventDto.EndAt)
            {
                throw new ValidationException();
            }
            if (createEventDto.TotalSeats <= 0) throw new ValidationException();
            Event evente = new Event
            (
                createEventDto.Title,
                createEventDto.Description,
                createEventDto.StartAt,
                createEventDto.EndAt,
                createEventDto.TotalSeats
            );
            events.Add(evente);
        }
        /// <summary>
        /// Удаление события по id
        /// </summary>
        /// <param name="id"></param>
        public void DeleteEvent(Guid id)
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
        public Event? GetEventById(Guid id)
        {
            return events.FirstOrDefault(e => e.Id == id, null) as Event;
            //return events.First(e => e.Id == id) as Event;//заглушка для теста

        }
        /// <summary>
        /// Обновить событие
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateEventDto"></param>
        public void UpdateEvent(Guid id, UpdateEventDto updateEventDto)
        {
            if (updateEventDto.StartAt > updateEventDto.EndAt)
            {
                throw new ValidationException();
            }
            Event? eventToUpdate = GetEventById(id);
            if (eventToUpdate != null)
            {
                int index = events.IndexOf(eventToUpdate);
                events[index] = new Event
                (
                    updateEventDto.Title,
                    updateEventDto.Description,
                    updateEventDto.StartAt,
                    updateEventDto.EndAt,
                    updateEventDto.TotalSeats
                );
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

        /// <summary>
        /// Получает отфильтрованный список событий
        /// </summary>
        /// <param name="title">регистронезависимое имя</param>
        /// <param name="from">дата начала</param>
        /// <param name="to">дата конца</param>
        /// <returns></returns>
        public PaginatedResult GetFilteredEvents(
            string title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize
            = 10)
        {
            var events = GetAllEvents();

            if (title != null)
            {
                events = events.Where(e => e.Title.Contains(title, StringComparison.InvariantCultureIgnoreCase));
            }

            if (from != null)
            {
                events = events.Where(e => e.StartAt >= from.Value);
            }

            if (to != null)
            {
                events = events.Where(e => e.EndAt <= to.Value);
            }
            //Общее число страниц/записей нужно считать по полной отфильтрованной выборке до Skip/Take
            int totalPages = (int)Math.Ceiling((double)events.Count() / pageSize);
            int totalEvents = events.Count();
            events = events.Skip((page - 1) * pageSize).Take(pageSize);

            return new PaginatedResult(totalEvents, events.ToList(), page, pageSize);
        }
    }
}
