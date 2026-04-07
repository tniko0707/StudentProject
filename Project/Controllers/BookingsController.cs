using Microsoft.AspNetCore.Mvc;
using Project.Models;

namespace Project.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bookingService"></param>
    [ApiController]
    [Route("/[controller]")]
    public class BookingsController(IBookingService bookingService): Controller
    {


        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var booking = bookingService.GetBookingByIdAsync(id);

            if (booking.Result == null) return NotFound();
            return Ok(booking);
        }
    }
}
