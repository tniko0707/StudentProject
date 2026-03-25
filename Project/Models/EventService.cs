using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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
                Id = 2,
                Title="имя2",
                Description="описание2",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddDays(3)
            },
            new Event()
            {
                Id = 3,
                Title="имя3",
                Description="описание3",
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
            if (createEventDto.StartAt > createEventDto.EndAt)
            {
                throw new ValidationException();
            }
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
            //return events.FirstOrDefault(e => e.Id == id) as Event;
            return events.First(e => e.Id == id) as Event;//заглушка для теста

        }
        /// <summary>
        /// Обновить событие
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateEventDto"></param>
        public void UpdateEvent(int id, UpdateEventDto updateEventDto)
        {
            if (updateEventDto.StartAt > updateEventDto.EndAt)
            {
                throw new ValidationException();
            }
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
            int pageSize = 10)
        {
            var events = this.GetAllEvents();

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
            
            events = events.Skip((page - 1) * pageSize).Take(pageSize);

            return new PaginatedResult(events.Count(), events.ToList(), page, pageSize);
        }
    }
}
