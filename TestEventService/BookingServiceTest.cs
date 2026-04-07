using Microsoft.Extensions.Logging;
using Moq;
using Project.Models;
using System;

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
            _bookingService = new BookingService(_mockRepository.Object);
            _eventService = new EventService();
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
        public async Task CreateBookingWithWrongId()
        {
            //arrange

            Guid eventId = Guid.NewGuid();
            _mockRepository.Setup(m => m.AddAsync(eventId)).ReturnsAsync((Booking)null);

            //act

        }

        [Fact]
        public async Task GetBookingWithWrongId()
        {
            //arrange
            Guid id = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.FindByIdAsync(It.IsAny<Guid>()))
                .Throws(new InvalidOperationException());

            //act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _bookingService.GetBookingByIdAsync(id));

            //assert
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
    }
}
