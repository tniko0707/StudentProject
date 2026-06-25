using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Extensions;
using System.Security.Claims;

namespace Presentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookingService"></param>
    [ApiController]
    [Route("/[controller]")]
    [Authorize]
    public class BookingsController : Controller
    {
        private CancellationToken _cancellationToken;
        private readonly IBookingService _bookingService;
        private readonly IEventService _eventService;
        private readonly IUserService _userService;
        public BookingsController(IBookingService bookingService,
            IEventService eventService, IUserService userService)
        {
            _cancellationToken = new CancellationTokenSource().Token;
            _bookingService = bookingService;
            _eventService = eventService;
            _userService = userService;
        }

        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetBooking")]
        public async Task<IActionResult> Get(Guid id)
        {
            Guid userId = User.GetUserId();
            var user = await _userService.FindByIdAsync(userId, _cancellationToken);

            var booking = await _bookingService.GetBookingByIdAsync(user, id, _cancellationToken);

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
            Guid userId = User.GetUserId();
            var user = await _userService.FindByIdAsync(userId, _cancellationToken);

            var eventt = await _eventService.GetEventByIdAsync(eventId, _cancellationToken);
            if (eventt == null) return NotFound();

            var booking = await _bookingService.CreateBookingAsync(user, eventId, _cancellationToken);

            return AcceptedAtRoute("GetBooking", new { Id = booking.Id }, booking);
        }

        [HttpDelete("{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            Guid userId = User.GetUserId();
            var user = await _userService.FindByIdAsync(userId, _cancellationToken);

            await _bookingService.CancelBooking(user, bookingId, _cancellationToken);

            return Ok("Бронь успешно отменена");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllBookingsAsync()
        {
            var r = await _bookingService.DeleteAllBookingsAsync(_cancellationToken);
            return Ok(r);
        }
    }
}
