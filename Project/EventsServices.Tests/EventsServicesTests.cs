using Events.Application.DTO;
using Events.Application.Repositories;
using Events.Application.Services;
using Events.Domain.Models;
using Moq;

namespace EventsServices.Tests
{
    public class EventsServicesTests
    {
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<IEventCacheRepository> _eventCacheRepository;
        private readonly EventService _eventService;


        public EventsServicesTests()
        {
            _eventCacheRepository = new Mock<IEventCacheRepository>();
            _eventRepository = new Mock<IEventRepository>();
            _eventService = new EventService(_eventRepository.Object, _eventCacheRepository.Object);
        }

        /// <summary>
        /// Есть в кэше
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEventById_NotUseRepository()
        {
            // Arrange
            var cachedEvent = new Event("newEvent",
                "описание",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                20);
            var ct = new CancellationToken();

            _eventCacheRepository.Setup(c => c.GetEventByIdAsync(cachedEvent.Id, ct))
                .ReturnsAsync(cachedEvent);

            // Act 
            var result = await _eventService.GetEventByIdAsync(cachedEvent.Id, ct);

            // Assert

            _eventRepository.Verify(r => r.GetByIdAsync(cachedEvent.Id, ct), Times.Never());
        }

        /// <summary>
        /// Нету в кэше
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetEventById_UseRepository()
        {
            // Arrange
            var cachedEvent = new Event("newEvent",
                "описание",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                20);
            var ct = new CancellationToken();

            _eventCacheRepository.Setup(c => c.GetEventByIdAsync(cachedEvent.Id, ct))
                .ReturnsAsync(cachedEvent);

            // Act 
            var result = await _eventService.GetEventByIdAsync(cachedEvent.Id, ct);

            // Assert
            _eventCacheRepository.Verify(r => r.GetEventByIdAsync(cachedEvent.Id, ct), Times.Once());
        }

        [Fact]
        public async Task UpdateEvent_DeleteKache()
        {
            // Arrange
            var cachedEvent = new Event("newEvent",
                "описание",
                DateTime.UtcNow.AddDays(1),
                DateTime.UtcNow.AddDays(2),
                20);
            var ct = new CancellationToken();

            var updateEventDto = new UpdateEventDto() { Title = "Новое название",
                Description = "новое описание",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2),
                AvailableSeats = 30 };

            _eventRepository.Setup(c => c.GetByIdAsync(cachedEvent.Id, ct)).ReturnsAsync(cachedEvent);

            // Act
            var result = await _eventService.UpdateEventAsync(cachedEvent.Id, updateEventDto, ct);

            // Assert
            _eventCacheRepository.Verify(r => r.DeleteKey(cachedEvent.Id), Times.Once());

        }
    }
}
