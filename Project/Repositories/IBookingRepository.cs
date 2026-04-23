using Project.Models;

namespace Project.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> FindByIdAsync(Guid id);
        Task<Booking> AddAsync(Guid eventId);
        Task<List<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetAllPendingAsync();
        Task<Booking> GetLastBookingAsync();

    }
}
