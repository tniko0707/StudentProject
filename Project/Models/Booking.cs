using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Booking
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public Guid EventId {  get; set; }
        [Required]
        public BookingStatus Status { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
