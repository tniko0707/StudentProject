using Events.Application.Repositories;
using Events.Application.Services;
using Events.Domain.Models;
using Moq;

namespace EventServiceTests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<IEventCacheRepository> _eventCacheRepository;
        private readonly EventService _eventService;


        public EventServiceTests()
        {
            _eventCacheRepository = new Mock<IEventCacheRepository>();
            _eventRepository = new Mock<IEventRepository>();
            _eventService = new EventService(_eventRepository.Object, _eventCacheRepository.Object);
        }

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
    }
}
