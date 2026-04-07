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
            var booking = new Booking()
            {
                Id = 3,
                EventId = 3,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-2),
                ProcessedAt = DateTime.Now.AddHours(-1),
            };
            int eventId = 3;
            _mockRepository.Setup(m => m.AddAsync(eventId)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.CreateBookingAsync(eventId);

            //assert
            Assert.Equal(booking, result);
        }

        [Fact]
        public async Task GetBookingById_ReturnBooking()
        {
            //arrange
            var booking = new Booking()
            {
                Id = 2,
                EventId = 2,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.Now.AddHours(-2),
                ProcessedAt = DateTime.Now.AddHours(-1),
            };
            int bookingId = 2;
            _mockRepository.Setup(m => m.FindByIdAsync(bookingId)).ReturnsAsync(booking);

            //act
            var result = await _bookingService.GetBookingByIdAsync(bookingId);

            //assert
            Assert.Equal(booking, result);
        }
        [Fact]
        public async Task CreateSeveralBookingsForOneEvent_AllIdsUnique()
        {
            //arrange
            int eventId = 2;
            int bookingId = 0;
            _mockRepository
                .Setup(m => m.AddAsync(It.IsAny<int>()))
                .ReturnsAsync((int eventId) => new Booking
                {
                    Id = bookingId++,
                    EventId = eventId,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTime.Now,
                    ProcessedAt = DateTime.Now
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

            var bookingId = 2;
            var booking = new Booking { Id = bookingId, Status = BookingStatus.Pending };
            _mockRepository.Setup(m => m.FindByIdAsync(bookingId)).ReturnsAsync(booking);

            //act
            await _bookingService.ConfirmBooking(bookingId);
            var updatedBooking = await _bookingService.GetBookingByIdAsync(bookingId);

            //assert
            Assert.Equal(BookingStatus.Confirmed, updatedBooking.Status);
        }

        [Fact]
        public async Task CreateBookingWithWrongId()
        {
            //arrange

            int eventId = 45;
            _mockRepository.Setup(m => m.AddAsync(eventId)).ReturnsAsync((Booking)null);

            //act

        }

        [Fact]
        public async Task GetBookingWithWrongId()
        {
            //arrange
            int id = 34;
            _mockRepository.Setup(repo => repo.FindByIdAsync(It.IsAny<int>()))
                .Throws(new InvalidOperationException());

            //act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _bookingService.GetBookingByIdAsync(id));

            //assert
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
    }
}
