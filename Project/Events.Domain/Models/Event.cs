using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Events.Domain.Models
{
    /// <summary>
    /// Событие
    /// </summary>
    public class Event
    {
        private Event()
        {
            Title = null!;
        }
        [SetsRequiredMembers]
        public Event(string title, string? description, DateTime? startAt,
            DateTime? endAt, int totalSeats)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = TotalSeats;
        }
        [Required]
        public Guid Id { get; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public required DateTime? StartAt { get; set; }
        [Required]
        public required DateTime? EndAt { get; set; }
        [Required]
        public int TotalSeats { get; set; }
        [Required]
        public int AvailableSeats { get; set; }
        //public List<Booking> Bookings { get; set; }

        /// <summary>
        /// Бронь места
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count) return false;
            AvailableSeats -= count;
            return true;
        }
        /// <summary>
        /// Освобождение мест при отклонении брони
        /// </summary>
        /// <param name="count"></param>
        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;
        }

        //internal void Update(UpdateEventDto updateEventDto)
        //{
        //    Title = updateEventDto.Title;
        //    Description = updateEventDto.Description;
        //    StartAt = updateEventDto.StartAt;
        //    EndAt = updateEventDto.EndAt;
        //    TotalSeats = updateEventDto.TotalSeats;
        //}
    }
}
