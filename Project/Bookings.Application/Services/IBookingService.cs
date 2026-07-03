using Bookings.Domain.Models;
using Users.Domain.Models;

namespace Bookings.Application.Services
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(User user, Guid eventId, CancellationToken cancellationToken);
        Task<Booking?> GetBookingByIdAsync(User user, Guid bookingId, CancellationToken cancellationToken);
        Task<bool> DeleteAllBookingsAsync(CancellationToken cancellationToken);
        Task<List<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken);
        Task<bool> CancelBooking(User user, Guid bookingId, CancellationToken cancellationToken);
    }
}
