using Events.Domain.Models;

namespace Events.Application.DTO
{
    public class EventResponseDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public static EventResponseDTO MapToDto(Event @event)
        {
            return new EventResponseDTO()
            {
                Id = @event.Id,
                Title = @event.Title,
                Description = @event.Description,
                StartAt = @event.StartAt,
                EndAt = @event.EndAt,
                TotalSeats = @event.TotalSeats,
                AvailableSeats = @event.AvailableSeats,
            };
        }
    }

}
