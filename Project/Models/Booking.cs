using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Booking
    {
        public Booking(Guid eventId, BookingStatus status, DateTime createdAt)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
        }

        [Required]
        public Guid Id { get;}
        [Required]
        public Guid EventId {  get; }
        [Required]
        public BookingStatus Status { get; set; }
        [Required]
        public DateTime CreatedAt { get;}
        public DateTime? ProcessedAt { get; set; }
    }
}
