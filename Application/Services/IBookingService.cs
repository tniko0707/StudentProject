using Domain.Models;

namespace Application.Services
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken);
        Task<bool> DeleteAllBookingsAsync(CancellationToken cancellationToken);
        Task<List<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken);
    }
}
