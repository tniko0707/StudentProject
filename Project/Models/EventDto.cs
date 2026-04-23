using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    [DateValidation(ErrorMessage = "Дата окончания события неверная")]
    public class CreateEventDto
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
        public DateTime? StartAt { get; set; } = null;
        [Required(ErrorMessage = "Дата окончания обязательна для заполнения")]
        public DateTime? EndAt { get; set; } = null;
        [Required]
        public int TotalSeats { get; set; }
    }

    [DateValidation(ErrorMessage = "Дата окончания события неверная")]
    public class UpdateEventDto
    {
        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }
        [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
        public DateTime? StartAt { get; set; } = null;
        [Required(ErrorMessage = "Дата окончания обязательна для заполнения")]
        public DateTime? EndAt { get; set; } = null;
        [Required]
        public int TotalSeats { get; set; }
        [Required]
        public int AvailableSeats { get; set; }
    }

}
