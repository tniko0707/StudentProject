using Application.Repositories;
using Domain.Models;
namespace Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Booking> CreateBookingAsync(Guid eventId, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                Event? eventForBooking = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
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
                await _bookingRepository.AddAsync(booking);
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
            //var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            var booking = await _bookingRepository.FindByIdAsync(bookingId, cancellationToken);
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
            return await _bookingRepository.GetAllAsync(cancellationToken);
        }
        /// <summary>
        /// Подтверждение брони
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public async Task ConfirmBookingAsync(Guid bookingId, CancellationToken cancellationToken)
        {
            await _bookingRepository.ConfirmBookingAsync(bookingId, cancellationToken);
        }
        /// <summary>
        /// Получение крайней брони
        /// </summary>
        /// <returns></returns>
        public async Task<Booking> GetLastBookingAsync(CancellationToken cancellationToken)
        {
            return await _bookingRepository.GetLastBookingAsync(cancellationToken);
        }
        /// <summary>
        /// Очистка база броней
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAllBookingsAsync(CancellationToken cancellationToken)
        {
            return await _bookingRepository.DeleteDataFromTable() > 0;
        }

    }
}
