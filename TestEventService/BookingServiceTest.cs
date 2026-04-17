using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Controllers;
using Project.Models;

namespace TestEventService
{
    public class BookingServiceTest
    {
        private readonly BookingService _bookingService;
        private readonly EventService _eventService;
        private readonly Mock<IBookingRepository> _mockRepository;
        public BookingServiceTest()
        {
            _mockRepository = new Mock<IBookingRepository>();
            _eventService = new EventService();
            _bookingService = new BookingService(_mockRepository.Object, _eventService);
        }
        public async Task CreateBookings_AfterCapacityExceeded_ThrowsNoAvailableSeatsException()
        {
            // Arrange
            var lastEvent = _eventService.GetLastEvent();

            // Создаём брони в пределах лимита
            var bookingsWithinCapacity = Enumerable.Range(1, 3)
                .Select(_ => new Booking
                {
                    Id = Guid.NewGuid(),
                    EventId = lastEvent.Id,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1),
                })
                .ToList();

            // Мокаем добавление брони — сначала возвращаем объект, потом выбрасываем исключение
            int bookingCount = 0;
            _mockRepository.Setup(m => m.AddAsync(It.IsAny<Guid>()))
                .Callback<Booking>(_ => bookingCount++) // Считаем вызовы метода
                .ReturnsAsync((Booking booking) =>
                {
                    // Если лимит превышен — выбрасываем исключение
                    if (bookingCount > 3)
                        throw new NoAvailableSeatsException();
                    return booking; // Иначе возвращаем бронь
                });

            // Act & Assert — успешно создаём брони до лимита
            foreach (var booking in bookingsWithinCapacity)
            {
                await _bookingService.CreateBookingAsync(lastEvent.Id); // Создаём брони
            }
            var r = _bookingService.CreateBookingAsync(lastEvent.Id);
            // Проверяем, что лимит достигнут
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await r);
                
        }
        [Fact]
        public async Task CreateSeveralBookings_AllHaveUniqueId()
        {
            //arrange
            Event lastEvent = _eventService.GetLastEvent();
            var bookings = new List<Booking>()
            {
                new Booking()
                {
                    Id = Guid.NewGuid(),
                    EventId = lastEvent.Id,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1),
                },
                new Booking()
                {
                    Id = Guid.NewGuid(),
                    EventId = lastEvent.Id,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1),
                },
                new Booking()
                {
                    Id = Guid.NewGuid(),
                    EventId = lastEvent.Id,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.Now.AddHours(-1),
                },
            };
            _mockRepository.Setup(m => m.AddAsync(It.IsAny<Guid>()));

            //act
            foreach (var booking in bookings)
            {
                await _bookingService.CreateBookingAsync(lastEvent.Id);
            }

            //assert
            _mockRepository.Verify(r => r.AddAsync(lastEvent.Id), Times.Exactly(3));
            var result = await _bookingService.GetAllBookingsAsync();
            Assert.Equal(3, result.Where(b => b.EventId == lastEvent.Id).Count());
        }
        [Fact]
        public async Task CreateBookingReducesAvailableSeats()
        {
            //arrange
            Event lastEvent = _eventService.GetLastEvent();
            int? availableSeats = lastEvent.AvailableSeats;
            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = lastEvent.Id,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-1),
            };
            _mockRepository.Setup(m => m.AddAsync(lastEvent.Id)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.CreateBookingAsync(lastEvent.Id);
            //assert
            Assert.Equal(lastEvent.AvailableSeats + 1, availableSeats);
        }
        [Fact]
        public async Task CreateBooking_ShouldReturnBookingInfo()
        {
            //arrange
            Guid eventId = _eventService.GetLastEvent().Id;
            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-2),
            };
            _mockRepository.Setup(m => m.AddAsync(eventId)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.CreateBookingAsync(eventId);

            //assert
            Assert.Equal(booking, result);
        }

        [Fact]
        public async Task GetBookingById_ReturnBooking()
        {
            Guid eventId = _eventService.GetLastEvent().Id;
            //arrange
            var booking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = Guid.NewGuid(),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-2),
                ProcessedAt = DateTime.Now.AddHours(-1),
            };
            _mockRepository.Setup(m => m.FindByIdAsync(booking.Id)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.GetBookingByIdAsync(booking.Id);

            //assert
            Assert.Equal(booking, result);
        }
        [Fact]
        public async Task CreateSeveralBookingsForOneEvent_AllIdsUnique()
        {
            //arrange
            Guid eventId = _eventService.GetLastEvent().Id;
            int bookingId = 0;
            _mockRepository
                .Setup(m => m.AddAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid eventId) => new Booking
                {
                    Id = Guid.NewGuid(),
                    EventId = eventId,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTime.Now,
                });
            //act

            var b1 = await _bookingService.CreateBookingAsync(eventId);
            var b2 = await _bookingService.CreateBookingAsync(eventId);

            //assert
            Assert.True(b1.Id != b2.Id);
        }

        [Fact]
        public async Task GetBookingStatusAfterConfirm_ReturnConfirmedStatus()
        {
            //arrange

            var bookingId = Guid.NewGuid();
            var booking = new Booking { Id = bookingId, Status = BookingStatus.Pending };
            _mockRepository.Setup(m => m.FindByIdAsync(bookingId)).ReturnsAsync(booking);

            //act
            await _bookingService.ConfirmBooking(booking.Id);
            var updatedBooking = await _bookingService.GetBookingByIdAsync(bookingId);

            //assert
            Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);
        }

        [Fact]
        public async Task CreateBookingWithWrongEventId()
        {
            //arrange

            Guid eventId = Guid.NewGuid();
            var mockEventService = new Mock<IEventService>();
            mockEventService.Setup(m => m.GetEventById(eventId)).Returns(null as Event);

            var controller = new EventsController(mockEventService.Object, _bookingService);
            //act
            var eventT = await controller.CreateBookingAsync(eventId);

            //assert
            Assert.IsType<NotFoundResult>(eventT);

        }

        [Fact]
        public async Task GetBookingWithWrongId()
        {
            //arrange
            Guid id = Guid.NewGuid();

            //act
            var booking = await _bookingService.GetBookingByIdAsync(id);

            //assert
            Assert.Null(booking);
        }
    }
}
