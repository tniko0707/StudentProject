namespace Events.Domain.Models
{
    /// <summary>
    /// Событие
    /// </summary>
    public class Event
    {
        private Event()
        {
        }
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
        public Guid Id { get; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public DateTime? StartAt { get; private set; }
        public DateTime? EndAt { get; private set; }
        public int TotalSeats { get; private set; }
        public int AvailableSeats { get; private set; }
        public double CalculateSalesPercent()
        {
            return Math.Round((double)(TotalSeats - AvailableSeats)/TotalSeats, 2);
        }
        public void Update(string title,
            string? description,
            DateTime? startAt,
            DateTime? endAt,
            int totalSeats)
        {
            var bookedSeats = TotalSeats - AvailableSeats;
            if (totalSeats < bookedSeats)
                throw new InvalidOperationException("Количество мест уже распроданных больше нового числа");

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
        }

        /// <summary>
        /// Бронь места
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public void TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count) throw new ArgumentException("Не хватает меcт");

            AvailableSeats -= count;
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
