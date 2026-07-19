using Events.Application.DTO;
using Events.Application.Repositories;
using Events.Domain.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Events.Infrastructure.Repositories
{
    public class EventCacheRepository : IEventCacheRepository
    {
        public readonly IDatabase _redis;
        private readonly IEventRepository _eventRepository;
        private readonly ILogger<EventCacheRepository> _logger;
        private static readonly TimeSpan ExpireTime = TimeSpan.FromMinutes(10);

        public EventCacheRepository(IDatabase database,
            IEventRepository eventRepository,
            ILogger<EventCacheRepository> logger)
        {
            _redis = database;
            _eventRepository = eventRepository;
            _logger = logger;
        }

        public async Task DeleteKey(Guid id)
        {
            var key = $"event:{id}";
            try
            {
                RedisValue cached = await _redis.KeyDeleteAsync(key);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении ключа события {id}");
            }
        }

        public async Task<EventResponseDTO?> GetEventByIdAsync(Guid id, CancellationToken ct)
        {
            var key = $"event:{id}";

            // Проверяем кэш
            try
            {
                RedisValue cached = await _redis.StringGetAsync(key);
                if (cached.HasValue)
                    return JsonSerializer.Deserialize<EventResponseDTO>(cached.ToString());
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка чтения ключа {key} из Redis при получении события");
            }

            // ПРомах - идем в базу
            Event? @event = await _eventRepository.GetByIdAsync(id, ct);
            if (@event is null) return null;
            var eventResponseDTO = EventResponseDTO.MapToDto(@event);
            //Кладем в кэш
            try
            {
                var serialized = JsonSerializer.Serialize(eventResponseDTO);
                await _redis.StringSetAsync(key, serialized, ExpireTime);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка записи ключа {key} из Redis при получении события");
            }

            return eventResponseDTO;
        }

        public async Task<List<EventResponseDTO>> GetTop10Events(CancellationToken ct)
        {
            var key = "events:top10";

            // Проверяем кэш
            try
            {
                RedisValue cache = await _redis.StringGetAsync(key);
                if (cache.HasValue)
                    return JsonSerializer.Deserialize<List<EventResponseDTO>>(cache.ToString()!) ?? new List<EventResponseDTO>();
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка чтения ключа {key} из Redis при получении события");
            }

            var events = await _eventRepository.GetTop10Async(ct);
            var dtos = events.Select(e => EventResponseDTO.MapToDto(e)).ToList();
            // Кладем в кэш
            try
            {
                var serialized = JsonSerializer.Serialize(dtos);
                await _redis.StringSetAsync(key, serialized, ExpireTime);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка записи ключа {key} из Redis при получении события");
            }

            return dtos;
        }

    }
}
