using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Project.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Booking
    {
        private Booking()
        {

        }
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
        [JsonIgnore]
        public Event? Event { get; set; }

        internal static Booking CreatePending(Guid eventId)
        {
            if (eventId == Guid.Empty) throw new ValidationException(nameof(eventId));
            return new Booking(eventId, BookingStatus.Pending, DateTime.UtcNow);

        }
    }
}
