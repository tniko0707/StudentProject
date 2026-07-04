using System.ComponentModel.DataAnnotations;

namespace Bookings.Domain.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Booking
    {
        private Booking()
        {

        }
        public Booking(Guid userId, Guid eventId, DateTime createdAt, int seatsCount)
        {
            UserId = userId;
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = createdAt;
            SeatsCount = seatsCount;
        }

        public Guid Id { get; }
        public Guid UserId { get; set; }
        public Guid EventId { get; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; }
        public DateTime? ProcessedAt { get; set; }
        public int SeatsCount { get; set; }

        public static Booking CreatePending(Guid userId, Guid eventId, int seatsCount)
        {
            if (userId == Guid.Empty) throw new ValidationException(nameof(userId));
            if (eventId == Guid.Empty) throw new ValidationException(nameof(eventId));
            return new Booking(userId, eventId, DateTime.UtcNow, seatsCount);
        }

        public void Reject()
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }

        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }

        public bool CancelBooking()
        {
            if (Status is BookingStatus.Cancelled) return false;
            Status = BookingStatus.Cancelled;
            ProcessedAt = DateTime.UtcNow;
            return true;
        }
    }
}
