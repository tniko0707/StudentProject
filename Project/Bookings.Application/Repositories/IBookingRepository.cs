using Bookings.Domain.Models;

namespace Bookings.Application.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> FindByIdAsync(Guid id, CancellationToken ct = default);
        Task<Booking> AddAsync(Booking booking, CancellationToken ct = default);
        Task<List<Booking>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<Booking>> GetAllPendingAsync(CancellationToken ct = default);
        Task<Booking> GetLastBookingAsync(CancellationToken ct = default);
        Task ConfirmBookingAsync(Guid id, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<int> DeleteDataFromTable(CancellationToken ct = default);
        Task<List<Booking>> GetAllUserBookings(Guid userId, CancellationToken ct = default);
    }
}
