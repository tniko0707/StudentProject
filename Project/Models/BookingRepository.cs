
using Microsoft.AspNetCore.Http.HttpResults;

namespace Project.Models
{
    public class BookingRepository : IBookingRepository
    {
        public readonly List<Booking> _bookings =
        [
            new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-1),
            },
            new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-2),
            }
        ];

        public async Task<Booking> AddAsync(Guid eventId)
        {
            Booking newBooking = new ()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
            };
            _bookings.Add(newBooking);
            return newBooking;
        }

        public async Task<Booking?> FindByIdAsync(Guid id)
        {
            return _bookings.FirstOrDefault(b => b.Id == id, null);
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return _bookings;
        }
        public async Task<IEnumerable<Booking>> GetAllPendingAsync()
        {
            return _bookings.Where(b => b.Status == BookingStatus.Pending);
        }
        public async Task<Booking> GetLastBookingAsync()
        {
            return _bookings.Last();
        }
    }
}
