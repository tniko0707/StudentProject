using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    /// <summary>
    /// Событие
    /// </summary>
    public class Event
    {
        public Event()
        {
            AvailableSeats = TotalSeats;
        }
        public Guid Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public required DateTime? StartAt { get; set; }
        [Required]
        public required DateTime? EndAt { get; set; }
        [Required]
        public int? TotalSeats { get; set; }
        [Required]
        public int? AvailableSeats {  get; set; }

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
    }
}
