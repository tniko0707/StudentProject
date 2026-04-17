
using System;
namespace Project.Models
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;
        private readonly IEventService _eventService;
        private readonly object _bookingLock = new();
        public BookingService(IBookingRepository repository, IEventService eventService)
        {
            _repository = repository;
            _eventService = eventService;
        }
        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Booking> CreateBookingAsync(Guid eventId)
        {
            lock(_bookingLock)
            {
                Event eventForBooking = _eventService.GetEventById(eventId);
                bool check = eventForBooking.TryReserveSeats();
                if (!check)
                {
                    throw new NoAvailableSeatsException();
                }
            }
            Booking booking = await _repository.AddAsync(eventId);
            return booking;
        }
        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            return await _repository.FindByIdAsync(bookingId);
        }
        /// <summary>
        /// Получить все брони
        /// </summary>
        /// <returns></returns>
        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _repository.GetAllAsync();
        }
        /// <summary>
        /// Подтверждение брони
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public async Task ConfirmBooking(Guid bookingId)
        {
            var b = await _repository.FindByIdAsync(bookingId);
            if (b != null) b.Status = BookingStatus.Confirmed;
        }
    }
}
