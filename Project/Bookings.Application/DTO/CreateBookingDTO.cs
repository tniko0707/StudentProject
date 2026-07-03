using Bookings.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Bookings.Application.DTO
{
    public class CreateBookingDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid EventId { get; }

        [Required]
        public BookingStatus Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; }
    }
}
