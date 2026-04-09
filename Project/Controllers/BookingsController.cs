using Microsoft.AspNetCore.Mvc;
using Project.Models;
using System.Threading.Tasks;

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
        [HttpGet("{id}", Name = "GetBooking")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var booking = await bookingService.GetBookingByIdAsync(id);
                return Ok(booking);
            }
            catch (InvalidOperationException ex) 
            {
                return NotFound();
            }
        }
    }
}
