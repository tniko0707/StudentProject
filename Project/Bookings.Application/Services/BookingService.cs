using Bookings.Application.Repositories;
using Bookings.Domain.Models;
using Events.Application.Repositories;
using Events.Domain.Models;
using Users.Domain.Models;
namespace Bookings.Application.Services
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
        public async Task<Booking> CreateBookingAsync(User user, Guid eventId, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var userBookings = await _bookingRepository.GetAllUserBookings(user.UserId, ct);
                if (userBookings.Where(b =>
                            b.Status == BookingStatus.Pending
                            || b.Status == BookingStatus.Confirmed).Count() >= 10)
                {
                    throw new BookingLimitException("Превышение событий у пользователя");
                }
                Event? eventForBooking = await _eventRepository
                    .FindByIdAsync(eventId, ct);
                if (eventForBooking == null)
                {
                    throw new KeyNotFoundException($"Событие {eventId} не найдено");
                }
                if (eventForBooking.StartAt <= DateTime.UtcNow)
                {
                    throw new BookingPastEventException("Событие уже началось");
                }

                bool check = eventForBooking.TryReserveSeats();
                if (!check)
                {
                    throw new NoAvailableSeatsException();
                }
                Booking booking = Booking.CreatePending(user.UserId, eventId);
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
        public async Task<Booking?> GetBookingByIdAsync(User user, Guid bookingId, CancellationToken cancellationToken)
        {
            //var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            var booking = await _bookingRepository.FindByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Событие {bookingId} не найдено");
            }
            if (booking.UserId != user.UserId)
            {
                throw new KeyNotFoundException($"Событие {bookingId} не относится к текущему пользователю");
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

        public async Task<bool> CancelBooking(User user, Guid bookingId, CancellationToken ct)
        {
            var booking = await _bookingRepository.FindByIdAsync(bookingId);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Событие {bookingId} не найдено");
            }

            bool isUserAdmin = user.Role == Role.Admin;

            if (!isUserAdmin && booking.UserId != user.UserId)
            {
                throw new NoRightsToChangeException("Нет прав для отмены");
            }

            if (booking.Event?.StartAt <= DateTime.UtcNow)
            {
                throw new BookingPastEventException("Событие уже началось");
            }

            booking.CancelBooking();
            await _bookingRepository.SaveChangesAsync(ct);

            return true;
        }
    }
}
