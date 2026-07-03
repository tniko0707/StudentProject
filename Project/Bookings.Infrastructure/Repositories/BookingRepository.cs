using Bookings.Application.Repositories;
using Bookings.Domain.Models;
using Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _db;

        public BookingRepository(AppDbContext db) { _db = db; }

        public async Task<Booking> AddAsync(Booking booking, CancellationToken ct = default)
        {
            _db.Bookings.Add(booking);
            await SaveChangesAsync(ct);
            return booking;
        }

        public async Task<Booking?> FindByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<List<Booking>> GetAllAsync(CancellationToken ct = default)
        {
            return await _db.Bookings.ToListAsync();
        }
        public async Task<IEnumerable<Booking>> GetAllPendingAsync(CancellationToken ct = default)
        {
            return await _db.Bookings.Where(b => b.Status == BookingStatus.Pending).ToListAsync();
        }
        public async Task<Booking> GetLastBookingAsync(CancellationToken ct = default)
        {
            return await _db.Bookings.OrderBy(b => b.CreatedAt).LastAsync();
        }
        public async Task ConfirmBookingAsync(Guid id, CancellationToken ct = default)
        {
            var b = await FindByIdAsync(id, ct);
            if (b != null)
            {
                b.Status = BookingStatus.Confirmed;
                await SaveChangesAsync(ct);
            }
            ;
        }
        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _db.SaveChangesAsync(ct);
        }
        public async Task<int> DeleteDataFromTable(CancellationToken ct = default)
        {
            return await _db.Bookings.ExecuteDeleteAsync(ct);
        }

        public async Task<List<Booking>> GetAllUserBookings(Guid userId, CancellationToken ct = default)
        {
            var bookings = await _db.Bookings.Where(b => b.UserId == userId).ToListAsync(ct);
            return bookings;
        }
    }
}
