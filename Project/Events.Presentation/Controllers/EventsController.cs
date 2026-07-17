using Events.Application.DTO;
using Events.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Events.Presentation.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    [Authorize]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private CancellationToken cancellationToken;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
            cancellationToken = new CancellationTokenSource().Token;
        }

        /// <summary>
        /// Получение событий через фильтр событий
        /// </summary>
        /// <param name="title">регистронезависимое имя</param>
        /// <param name="from">дата начала</param>
        /// <param name="to">дата конца</param>
        /// <param name="page">1</param>
        /// <param name="pageSize">10</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string title = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var events = await _eventService.GetFilteredEventsAsync(title, from, to, page, pageSize);

            return Ok(events);
        }

        /// <summary>
        /// Получение события по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var evente = await _eventService.GetEventByIdAsync(id, cancellationToken);
            if (evente == null) return NotFound();
            return Ok(evente);
        }

        /// <summary>
        /// Получение ТОП 10 популярных событий
        /// </summary>
        /// <returns></returns>
        [HttpGet("top")]
        public async Task<IActionResult> GetTop10()
        {
            var events = await _eventService.GetTop10EventsAsync(cancellationToken);
            return Ok(events);
        }

        /// <summary>
        /// Создание события
        /// </summary>
        /// <param name="createEventDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateEventDto createEventDto)
        {
            //if (!ModelState.IsValid) return BadRequest();
            if (!ModelState.IsValid)
            {
                var problemDetails = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                };
                return BadRequest(problemDetails);
            }
            var ev = await _eventService.CreateEventAsync(createEventDto, cancellationToken);
            //return new CreatedAtActionResult(nameof(Get), nameof(Get), new {id = ev.Id}, ev);
            return CreatedAtAction(nameof(Create), new { id = ev.Id }, ev);
        }
        /// <summary>
        /// Обновление события
        /// </summary>
        /// <param name="id">Id события</param>
        /// <param name="evente"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventDto updateEventDto)
        {
            if (await _eventService.GetEventByIdAsync(id, cancellationToken) == null) return NotFound();
            await _eventService.UpdateEventAsync(id, updateEventDto, cancellationToken);
            return new NoContentResult();
        }
        /// <summary>
        /// Удаление события по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (await _eventService.GetEventByIdAsync(id, cancellationToken) == null) return NotFound();
            await _eventService.DeleteEventAsync(id, cancellationToken);
            return new OkResult();
        }

    }
}
