namespace Bookings.Domain.Models
{
    public class BookingPastEventException : Exception
    {
        public BookingPastEventException() : base("Event already passed") { }
        public BookingPastEventException(string message) : base(message) { }
        public BookingPastEventException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
