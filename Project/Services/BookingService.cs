using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.DataAccess;
using Project.Models;
namespace Project.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly AppDbContext _dbContext;
        public BookingService(AppDbContext dbContext) { _dbContext = dbContext; }

        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            //lock(_bookingLock)
            try
            {
                //Event eventForBooking = _eventService.GetEventById(eventId);
                Event? eventForBooking = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == eventId, 
                    cancellationToken);
                if (eventForBooking == null)
                {
                    throw new KeyNotFoundException($"Событие {eventId} не найдено");
                }
                bool check = eventForBooking.TryReserveSeats();
                if (!check)
                {
                    throw new NoAvailableSeatsException();
                }
                Booking booking = Booking.CreatePending(eventId);
                await _dbContext.Bookings.AddAsync(booking, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return booking;
            }
            finally { _semaphore.Release(); }
        }
        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Событие {bookingId} не найдено");
            }
            return booking;
        }
        /// <summary>
        /// Получить все брони
        /// </summary>
        /// <returns></returns>
        public async Task<List<Booking>> GetAllBookingsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Bookings.ToListAsync(cancellationToken);
        }
        /// <summary>
        /// Подтверждение брони
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public async Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            var b = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (b != null)
            {
                b.Status = BookingStatus.Confirmed;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            ;
        }
        /// <summary>
        /// Получение крайней брони
        /// </summary>
        /// <returns></returns>
        public async Task<Booking> GetLastBookingAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Bookings.LastAsync(cancellationToken);
        }
        /// <summary>
        /// Очистка база броней
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAllBookingsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Bookings.ExecuteDeleteAsync(cancellationToken) > 0;
        }

    }
}
