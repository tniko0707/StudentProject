using Bookings.Application.DTO;
using Bookings.Domain.Models;

namespace Bookings.Application.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDTO> CreateAsync(Guid userId,
            CreateBookingDTO createBookingDTO,
            CancellationToken ct = default);
        Task<BookingResponseDTO> GetByIdAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken);
        Task<BookingResponseDTO> GetLastBookingAsync(CancellationToken cancellationToken);
        Task<bool> DeleteAllBookingsAsync(CancellationToken cancellationToken);
        Task<List<BookingResponseDTO>> GetAllBookingsAsync(CancellationToken cancellationToken);
        Task<bool> CancelAsync (Guid userId, Guid bookingId, CancellationToken cancellationToken);
    }
}
