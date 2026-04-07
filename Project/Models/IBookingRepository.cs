namespace Project.Models
{
    public interface IBookingRepository
    {
        Task<Booking?> FindByIdAsync(int id);
        Task<Booking> AddAsync(int eventId);
    }
}
