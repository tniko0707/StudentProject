
using System;
namespace Project.Models
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;
        public BookingService(IBookingRepository repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Booking?> CreateBookingAsync(Guid eventId)
        {
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
