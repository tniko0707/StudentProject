namespace Project.Models
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(Guid eventId);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    }
}
