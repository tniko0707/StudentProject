
using Microsoft.AspNetCore.Http.HttpResults;

namespace Project.Models
{
    public class BookingRepository : IBookingRepository
    {
        public readonly List<Booking> _bookings =
        [
            new Booking()
            {
                Id = 1,
                EventId = 1,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-1),
                ProcessedAt = DateTime.Now,
            },
            new Booking()
            {
                Id = 2,
                EventId = 2,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-2),
                ProcessedAt = DateTime.Now.AddHours(-1),
            }
        ];
        public async Task<Booking> AddAsync(int eventId)
        {
            Booking newBooking = new ()
            {
                Id = _bookings.Max(b => b.Id) + 1,
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now,
                ProcessedAt = DateTime.Now,
            };
            _bookings.Add(newBooking);
            return newBooking;
        }

        public async Task<Booking?> FindByIdAsync(int id)
        {
            try
            {
                return _bookings.First(b => b.Id == id);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
