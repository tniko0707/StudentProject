using Bookings.Application.DTO;
using Bookings.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Users.Application.Services;

namespace Bookings.Presentation.Controllers
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
        public BookingsController(IBookingService bookingService)
        {
            _cancellationToken = new CancellationTokenSource().Token;
            _bookingService = bookingService;
        }

        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetBooking")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = Guid.Parse(userIdClaim!);

            var booking = await _bookingService.GetByIdAsync(userId, id, _cancellationToken);

            if (booking == null) return NotFound();
            return Ok(booking);
        }

        /// <summary>
        /// Создание брони для события
        /// </summary>
        /// <param name = "eventId" > id события</param>
        /// <returns> Бронь </returns >
        [HttpPost]
        public async Task<IActionResult> CreateBookingAsync([FromBody] CreateBookingDTO createBookingDTO)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = Guid.Parse(userIdClaim!);

            var booking = await _bookingService.CreateAsync(userId, createBookingDTO, _cancellationToken);

            return StatusCode(StatusCodes.Status201Created, booking);
        }

        [HttpDelete("{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(Guid bookingId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = Guid.Parse(userIdClaim!);

            if (!await _bookingService.CancelAsync(userId, bookingId, _cancellationToken))
            {
                return NotFound();
            }

            return Ok("Бронь успешно отменена");
        }

        [HttpDelete]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> DeleteAllBookingsAsync()
        {
            var r = await _bookingService.DeleteAllBookingsAsync(_cancellationToken);
            return Ok(r);
        }
    }
}
