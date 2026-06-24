namespace Domain.Models
{
    public class BookingLimitException : Exception
    {
        public BookingLimitException() : base("Out of available seats") { }
        public BookingLimitException(string? message) : base(message)
        {
        }
        public BookingLimitException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
