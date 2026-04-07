namespace Project.Models
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(int eventId);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
    }
}
