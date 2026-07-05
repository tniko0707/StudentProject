using Bookings.Application.DTO;
using Bookings.Application.Repositories;
using Bookings.Domain.Models;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Messages;
namespace Bookings.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IKafkaProducer _kafkaProducer;
        public BookingService(IBookingRepository bookingRepository, IKafkaProducer kafkaProducer)
        {
            _bookingRepository = bookingRepository;
            _kafkaProducer = kafkaProducer;
        }

        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="eventId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception> 
        public async Task<BookingResponseDTO> CreateAsync(Guid userId, 
            CreateBookingDTO createBookingDTO, 
            CancellationToken ct = default)
        {
            Booking booking = Booking.CreatePending(userId, createBookingDTO.EventId, createBookingDTO.SeatsCount);

            booking.Confirm();

            await _bookingRepository.AddAsync(booking, ct);

            var message = new BookingConfirmedEvent()
            {
                BookingId = booking.Id,
                UserId = userId,
                EventId = createBookingDTO.EventId,
                SeatsNumber = createBookingDTO.SeatsCount,
            };

            await _kafkaProducer.PublishBookingConfirmedAsync(message);

            return MapToDTO(booking);

        }

        private static BookingResponseDTO MapToDTO(Booking booking)
        {
            return new BookingResponseDTO()
            {
                Id = booking.Id,
                UserId = booking.UserId,
                EventId = booking.EventId,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt,
                SeatsCount = booking.SeatsCount,
                Status = booking.Status,
            };
        }

        /// <summary>
        /// Получение брони по id
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        public async Task<BookingResponseDTO> GetByIdAsync(Guid userId, Guid bookingId, CancellationToken cancellationToken)
        {
            //var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null)
            {
                throw new KeyNotFoundException($"Событие {bookingId} не найдено");
            }
            if (booking.UserId != userId)
            {
                throw new KeyNotFoundException($"Событие {bookingId} не относится к текущему пользователю");
            }
            return MapToDTO(booking);
        }

        /// <summary>
        /// Получить все брони
        /// </summary>
        /// <returns></returns>
        public async Task<List<BookingResponseDTO>> GetAllBookingsAsync(CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetAllAsync(cancellationToken);
            return bookings.Select(b => MapToDTO(b)).ToList();
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
        public async Task<BookingResponseDTO> GetLastBookingAsync(CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetLastBookingAsync(cancellationToken);
            return MapToDTO(booking);
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

        public async Task<bool> CancelAsync(Guid userId, Guid bookingId, CancellationToken ct)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new KeyNotFoundException($"Событие {bookingId} не найдено");

            if (booking.UserId != userId)
            {
                throw new UnauthorizedAccessException("Нет прав для отмены");
            }

            if (!booking.CancelBooking()) return false;

            await _bookingRepository.UpdateAsync(booking, ct);

            return true;
        }
    }
}
