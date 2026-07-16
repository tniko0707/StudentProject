using System.ComponentModel.DataAnnotations;

namespace Bookings.Application.DTO
{
    public class CreateBookingDTO
    {

        [Required]
        public Guid EventId { get; set; }

        [Required]
        [Range(1, 10, ErrorMessage = "Число мест должно быть больше 1")]
        public int SeatsCount { get; set; }
    }
}
