using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly IBookingTaskQueue _bookingTaskQueue;

        public EventsController(IEventService eventService, IBookingService bookingService, IBookingTaskQueue bookingTaskQueue)
        {
            _eventService = eventService;
            _bookingService = bookingService;
            _bookingTaskQueue = bookingTaskQueue;
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
        public IActionResult Get(
            [FromQuery] string title = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var events = _eventService.GetFilteredEvents(title, from, to, page, pageSize);

            return Ok(events);
        }
        /// <summary>
        /// Получение события по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var evente = _eventService.GetEventById(id);
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
            //if (!ModelState.IsValid) return BadRequest();
            if (!ModelState.IsValid)
            {
                var problemDetails = new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                };
                return BadRequest(problemDetails);
            }
            _eventService.CreateEvent(createEventDto);
            Event ev = _eventService.GetLastEvent();
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
        public IActionResult Update(Guid id, [FromBody]UpdateEventDto updateEventDto)
        {
            if (_eventService.GetEventById(id) == null) return NotFound();
            _eventService.UpdateEvent(id, updateEventDto);
            return new NoContentResult();
        }
        /// <summary>
        /// Удаление события по Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            if (_eventService.GetEventById(id) == null) return NotFound();
            _eventService.DeleteEvent(id);
            return new OkResult();
        }

        /// <summary>
        /// Создание брони для события
        /// </summary>
        /// <param name = "eventId" > id события</param>
        /// <returns> Бронь </returns >
        [HttpPost("{eventId}/book")]
        public async Task<IActionResult> CreateBookingAsync(Guid eventId)
        {
            if (_eventService.GetEventById(eventId) == null) return NotFound();

            var booking = await _bookingService.CreateBookingAsync(eventId);

            _bookingTaskQueue.Enqueue(new BookingTask() { Id = booking.Id, CreatedAt = DateTime.Now });

            return AcceptedAtRoute("GetBooking", new { bookingId = booking.Id }, booking);
        }

    }
}
