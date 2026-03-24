using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class EventsController (IEventService eventService) : Controller
    {
        /// <summary>
        /// Получение списка событий
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(eventService.GetAllEvents());
        }
        /// <summary>
        /// Получение события по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var evente = eventService.GetEventById(id);
            //if (evente == null) return NotFound();
            return Ok(evente);
        }
        /// <summary>
        /// Создание события
        /// </summary>
        /// <param name="createEventDto"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Create([FromBody] CreateEventDto createEventDto)
        {
            if (!ModelState.IsValid) return BadRequest();
            eventService.CreateEvent(createEventDto);
            Event ev = eventService.GetLastEvent();
            //return new CreatedAtActionResult(nameof(Get), nameof(Get), new {id = ev.Id}, ev);
            return CreatedAtAction(nameof(Create), new {id = ev.Id}, ev);
        }
        /// <summary>
        /// Обновление события
        /// </summary>
        /// <param name="id">Id события</param>
        /// <param name="evente"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UpdateEventDto updateEventDto)
        {
            if (eventService.GetEventById(id) == null) return NotFound();
            eventService.UpdateEvent(id, updateEventDto);
            return new NoContentResult();
        }
        /// <summary>
        /// Удаление события по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (eventService.GetEventById(id) == null) return NotFound();
            eventService.DeleteEvent(id);
            return new OkResult();
        }
        /// <summary>
        /// Фильтр событий
        /// </summary>
        /// <param name="title">регистронезависимое имя</param>
        /// <param name="from">дата начала</param>
        /// <param name="to">дата конца</param>
        /// <param name="page">1</param>
        /// <param name="pageSize">10</param>
        /// <returns></returns>
        [HttpGet("filter")]
        public IActionResult GetFilteredEvents(
            [FromQuery] string title = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var events = eventService.GetFilteredEvents(title, from, to, page, pageSize);

            return Ok(events);
        }
    }
}
