using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookingService"></param>
    [ApiController]
    [Route("/[controller]")]
    public class BookingsController : Controller
    {
        private CancellationToken _cancellationToken;
        private readonly IBookingService _bookingService;
        private readonly IEventService _eventService;
        public BookingsController(IBookingService bookingService,
            IEventService eventService)
        {
            _cancellationToken = new CancellationTokenSource().Token;
            _bookingService = bookingService;
            _eventService = eventService;
        }

        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetBooking")]
        public async Task<IActionResult> Get(Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id, _cancellationToken);

            if (booking == null) return NotFound();
            return Ok(booking);
        }

        /// <summary>
        /// Создание брони для события
        /// </summary>
        /// <param name = "eventId" > id события</param>
        /// <returns> Бронь </returns >
        [HttpPost("{eventId}/book")]
        public async Task<IActionResult> CreateBookingAsync(Guid eventId)
        {
            if (await _eventService.GetEventByIdAsync(eventId, _cancellationToken) == null) return NotFound();

            var booking = await _bookingService.CreateBookingAsync(eventId, _cancellationToken);

            return AcceptedAtRoute("GetBooking", new { Id = booking.Id }, booking);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllBookingsAsync()
        {
            var r = await _bookingService.DeleteAllBookingsAsync(_cancellationToken);
            return Ok(r);
        }
    }
}
