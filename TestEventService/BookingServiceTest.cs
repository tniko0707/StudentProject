using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Controllers;
using Project.Models;
using Project.Repositories;
using Project.Services;

namespace TestEventService
{
    public class BookingServiceTest
    {
        private readonly BookingService _bookingService;
        private EventService _eventService;
        private readonly Mock<IBookingRepository> _mockRepository;
        public BookingServiceTest()
        {

            _mockRepository = new Mock<IBookingRepository>();
            _eventService = new EventService();
            _bookingService = new BookingService(_mockRepository.Object, _eventService);
        }
        //[Fact]
        //public async Task CreateBooking_NoAvailableSeats_ThrowsNoAvailableSeatsException()
        //{
        //    _eventService = new EventService();
        //    // Arrange
        //    var lastEvent = _eventService.GetLastEvent();
        //    int initialAvailableSeats = lastEvent.AvailableSeats;

        //    // Настраиваем мок репозитория: имитируем событие с лимитом мест

        //    // Создаём брони для исчерпания мест
        //    for (int i = 0; i < initialAvailableSeats; i++)
        //    {
        //        await _bookingService.CreateBookingAsync(lastEvent.Id);
        //    }

        //    // Act и Assert
        //    // Проверяем, что следующая попытка создать бронь выбрасывает исключение
        //    await Assert.ThrowsAsync<NoAvailableSeatsException>(async () =>
        //    {
        //        await _bookingService.CreateBookingAsync(lastEvent.Id);
        //    });
        //}
        [Fact]
        public async Task CreateBooking_WithNoAvailableSeats()
        {
            //arrange
            _eventService.CreateEvent(new CreateEventDto()
            {
                Title = "test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3),
                TotalSeats = 1,
            });
            Event lastEvent = _eventService.GetLastEvent();
            //act
            await _bookingService.CreateBookingAsync(lastEvent.Id);
            //assert
            await Assert.ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(lastEvent.Id));

        }

        [Fact]
        public async Task GetAllBookingsAsync_ReturnBookings()
        {
            Event lastEvent = _eventService.GetLastEvent();
            var bookingsMock = new List<Booking>()
            {
                new Booking(
                    lastEvent.Id,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-1)
                ),
                new Booking(
                    lastEvent.Id,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-2)
                ),
                new Booking(
                    lastEvent.Id,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-3)
                ),
            };

            //arrange
            _mockRepository.Setup(m => m.GetAllAsync()).ReturnsAsync(bookingsMock);

            //act
            var bookings = await _bookingService.GetAllBookingsAsync();

            //assert
            Assert.NotEmpty(bookings);
        }
        [Fact]
        public async Task CreateSeveralBookings_AllHaveUniqueId()
        {
            //arrange
            _eventService.CreateEvent(new CreateEventDto()
            {
                Title = "test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3),
                TotalSeats = 4,
            });
            Event lastEvent = _eventService.GetLastEvent();
            _mockRepository
                .Setup(m => m.AddAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid eventId) => new Booking
                (
                    eventId,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-2)
                ));
            var bookings = new List<Booking>()
            {
                new Booking(
                    lastEvent.Id,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-1)
                ),
                new Booking(
                    lastEvent.Id,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-2)
                ),
                new Booking(
                    lastEvent.Id,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-3)
                ),
            };
            _mockRepository.Setup(m => m.GetAllAsync()).ReturnsAsync(bookings);

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
            var booking = new Booking
            (
                lastEvent.Id,
                BookingStatus.Pending,
                DateTime.Now.AddHours(-1)
            );
            _mockRepository.Setup(m => m.AddAsync(lastEvent.Id)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.CreateBookingAsync(lastEvent.Id);
            //assert
            Assert.Equal(lastEvent.AvailableSeats + 1, availableSeats);
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
        public async Task GetBookingStatusAfterConfirm_ReturnConfirmedStatus()
        {
            //arrange
            Guid eventId = _eventService.GetLastEvent().Id;
            Booking booking = new Booking(eventId, BookingStatus.Pending, DateTime.Now);
            _mockRepository.Setup(m => m.FindByIdAsync(booking.Id)).ReturnsAsync(booking);

            //act
            await _bookingService.ConfirmBooking(booking.Id);
            var updatedBooking = await _bookingService.GetBookingByIdAsync(booking.Id);

            //assert
            Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);
        }
        [Fact]
        public async Task GetBookingById_ReturnBooking()
        {
            Guid eventId = _eventService.GetLastEvent().Id;
            //arrange
            var booking = new Booking
            (
                eventId,
                BookingStatus.Pending,
                DateTime.Now.AddHours(-2)
            );
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
            Event eventt = _eventService.GetLastEvent();
            _mockRepository
                .Setup(m => m.AddAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid eventId) => new Booking
                (
                    eventId,
                    BookingStatus.Pending,
                    DateTime.Now.AddHours(-2)
                ));
            //act

            var b1 = await _bookingService.CreateBookingAsync(eventt.Id);
            var b2 = await _bookingService.CreateBookingAsync(eventt.Id);

            //assert
            Assert.True(b1.Id != b2.Id);
        }
        [Fact]
        public async Task CreateBooking_ShouldReturnBookingInfo()
        {
            //arrange
            _eventService.CreateEvent(new CreateEventDto()
            {
                Title = "test",
                Description = "Test",
                StartAt = DateTime.Now,
                EndAt = DateTime.Now.AddHours(3),
                TotalSeats = 4,
            });
            //arrange
            Guid eventId = _eventService.GetLastEvent().Id;
            var booking = new Booking
            (
                eventId,
                BookingStatus.Pending,
                DateTime.Now.AddHours(-2)
            );
            _mockRepository.Setup(m => m.AddAsync(eventId)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.CreateBookingAsync(eventId);

            //assert
            Assert.Equal(booking, result);
        }
    }
}
