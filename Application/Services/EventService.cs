using Application.DTO;
using Application.Repositories;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }
        /// <summary>
        /// Создать событие
        /// </summary>
        /// <param name="createEventDto"></param>
        /// <returns></returns>
        public async Task<Event> CreateEventAsync(CreateEventDto createEventDto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(createEventDto.Title) ||
                string.IsNullOrWhiteSpace(createEventDto.Title) ||
                createEventDto.StartAt == null ||
                createEventDto.EndAt == null ||
                createEventDto.StartAt < DateTime.UtcNow ||
                createEventDto.StartAt >= createEventDto.EndAt)
            {
                throw new ValidationException();
            }
            if (createEventDto.TotalSeats <= 0) throw new ValidationException();
            Event evente = new Event
            (
                createEventDto.Title.TrimEnd().TrimStart(),
                createEventDto.Description,
                createEventDto.StartAt,
                createEventDto.EndAt,
                createEventDto.TotalSeats
            );
            await _eventRepository.AddAsync(evente);
            //await _dbContext.Events.AddAsync(evente, cancellationToken);
            //await _dbContext.SaveChangesAsync(cancellationToken);
            return evente;
        }
        /// <summary>
        /// Удаление события по id
        /// </summary>
        /// <param name="id"></param>
        public async Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken)
        {
            //Event @event = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            Event? @event = await _eventRepository.FindByIdAsync(id, cancellationToken);
            if (@event == null)
            {
                return false;
            }
            await _eventRepository.RemoveEvent(@event, cancellationToken);
            //_dbContext.Events.Remove(@event);
            //await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        /// <summary>
        /// Получить все события
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Event>> GetAllEventsAsync(CancellationToken cancellationToken)
        {
            return await _eventRepository.GetAll(cancellationToken);
            //return await _dbContext.Events.ToListAsync(cancellationToken);
        }
        /// <summary>
        /// Получить событие по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Event> GetEventByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            Event? eventForBooking = await _eventRepository.FindByIdAsync(id, cancellationToken);
            //var eventForBooking = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken) as Event;
            if (eventForBooking == null)
            {
                throw new KeyNotFoundException($"Событие {id} не найдено");
            }
            return eventForBooking;
            //return events.First(e => e.Id == id) as Event;//заглушка для теста

        }
        /// <summary>
        /// Обновить событие
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateEventDto"></param>
        public async Task<Event?> UpdateEventAsync(Guid id, UpdateEventDto updateEventDto, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(updateEventDto.Title) ||
                updateEventDto.Title.Length == 0 ||
                updateEventDto.StartAt == null ||
                updateEventDto.EndAt == null ||
                updateEventDto.StartAt < DateTime.UtcNow ||
                updateEventDto.StartAt >= updateEventDto.EndAt)
            {
                throw new ValidationException();
            }
            Event? eventToUpdate = await GetEventByIdAsync(id, cancellationToken);
            if (eventToUpdate == null)
            {
                throw new KeyNotFoundException($"Событие {id} не найдено");
            }
            if (eventToUpdate != null)
            {
                eventToUpdate.Title = updateEventDto.Title;
                eventToUpdate.Description = updateEventDto.Description;
                eventToUpdate.StartAt = updateEventDto.StartAt;
                eventToUpdate.EndAt = updateEventDto.EndAt;
                eventToUpdate.TotalSeats = updateEventDto.TotalSeats;
            }
            //await _dbContext.SaveChangesAsync(cancellationToken);
            await _eventRepository.SaveChangesAsync(cancellationToken);
            return eventToUpdate;
        }
        /// <summary>
        /// Получить последнее событие
        /// </summary>
        /// <returns></returns>
        public async Task<Event?> GetLastEventAsync(CancellationToken cancellationToken)
        {
            //return await _dbContext.Events.LastAsync(cancellationToken);
            return await _eventRepository.GetLast(cancellationToken);
        }

        /// <summary>
        /// Получает отфильтрованный список событий
        /// </summary>
        /// <param name="title">регистронезависимое имя</param>
        /// <param name="from">дата начала</param>
        /// <param name="to">дата конца</param>
        /// <param name="page">номер страницы для показа</param>
        /// <param name="pageSize">число элементов на странице</param>
        /// <param name="cancellationToken">токен отмены</param>
        /// <returns></returns>
        public async Task<PaginatedResult> GetFilteredEventsAsync(
            string? title = null,
            DateTime? from = null,
            DateTime? to = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            //var query = _dbContext.Events.AsQueryable();
            var query = _eventRepository.GetQuery();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(e => EF.Functions.Like(e.Title, $"%{title}%"));
            }

            if (from != null)
            {
                query = query.Where(e => e.StartAt >= from.Value);
            }

            if (to != null)
            {
                query = query.Where(e => e.EndAt <= to.Value);
            }
            //Общее число страниц/записей нужно считать по полной отфильтрованной выборке до Skip/Take
            int totalEvents = await query.CountAsync(cancellationToken);
            int totalPages = (int)Math.Ceiling((double)totalEvents / pageSize);
            query = query.Skip((page - 1) * pageSize).Take(pageSize);

            return new PaginatedResult(totalEvents, await query.ToListAsync(cancellationToken), page, pageSize, totalPages);
        }
    }
}
