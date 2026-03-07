using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    [DateValidation(ErrorMessage = "Дата окончания события неверная")]
    public class Event
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        public string? Description { get; set; }
        [Required]
        public required DateTime StartAt { get; set; }
        [Required]
        public required DateTime EndAt { get; set; }
    }
}
