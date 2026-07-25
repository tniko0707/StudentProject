using Events.Application.DTO;
using Events.Application.Repositories;
using Events.Application.Services;
using Events.Domain.Models;
using Events.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System.Threading;

namespace EventsServices.Tests
{
    public class EventsServicesTests
    {
        private readonly Mock<IDatabase> _redisDB;
        private readonly Mock<IEventRepository> _eventRepository;
        private readonly Mock<IEventCacheRepository> _eventCacheRepositoryMock;
        private readonly EventCacheRepository _eventCacheRepository;
        private readonly EventService _eventService;


        public EventsServicesTests()
        {
            _eventRepository = new Mock<IEventRepository>();
            _eventCacheRepositoryMock = new Mock<IEventCacheRepository>();
            _eventService = new EventService(_eventRepository.Object, _eventCacheRepositoryMock.Object);

            var loggerMock = new Mock<ILogger<EventCacheRepository>>();
            _redisDB = new Mock<IDatabase>();
            // Создаем НАСТОЯЩИЙ объект EventCacheRepository, передавая ему наши моки
            _eventCacheRepository = new EventCacheRepository(
                _redisDB.Object,
                _eventRepository.Object,
                loggerMock.Object);
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
            var dto = EventResponseDTO.MapToDto(cachedEvent);
            var ct = new CancellationToken();

            _eventCacheRepositoryMock.Setup(c => c.GetEventByIdAsync(cachedEvent.Id, ct))
                .ReturnsAsync(dto);

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
            var key = $"event:{cachedEvent.Id}";

            var ct = new CancellationToken();

            _redisDB
                .Setup(db => db.StringGetAsync(It.Is<RedisKey>(k => k == key), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            _eventRepository
                .Setup(repo => repo.GetByIdAsync(cachedEvent.Id, ct))
                .ReturnsAsync(cachedEvent);

            // Act 
            var result = await _eventCacheRepository.GetEventByIdAsync(cachedEvent.Id, ct);

            // Assert
            _eventRepository.Verify(
                repo => repo.GetByIdAsync(cachedEvent.Id, ct),
                Times.Once);

            var setInvocation = _redisDB.Invocations
                .FirstOrDefault(i => i.Method.Name == "StringSetAsync");

            Assert.NotNull(setInvocation);
            Assert.Equal((RedisKey)key, (RedisKey)setInvocation.Arguments[0]);
            var value = (RedisValue)setInvocation.Arguments[1];
            Assert.True(value.HasValue && !value.IsNullOrEmpty);

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
            _eventCacheRepositoryMock.Verify(r => r.DeleteKey(cachedEvent.Id), Times.Once());

        }
    }
}
