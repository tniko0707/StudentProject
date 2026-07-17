using Events.Application.Repositories;
using Events.Domain.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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

        public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken ct)
        {
            var key = $"event:{id}";

            // Проверяем кэш
            try
            {
                RedisValue cached = await _redis.StringGetAsync(key);
                if (cached.HasValue)
                    return JsonSerializer.Deserialize<Event>(cached!);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка чтения ключа {key} из Redis при получении события");
            }

            // ПРомах - идем в базу
            Event? @event = await _eventRepository.GetByIdAsync(id, ct);
            if (@event is null) return null;

            //Кладем в кэш
            try
            {
                var serialized = JsonSerializer.Serialize(@event);
                await _redis.StringSetAsync(key, serialized, ExpireTime);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка записи ключа {key} из Redis при получении события");
            }

            return @event;
        }

        public async Task<List<Event>> GetTop10Events(CancellationToken ct)
        {
            var key = "events:top10";

            // Проверяем кэш
            try
            {
                RedisValue cache = await _redis.StringGetAsync(key);
                if (cache.HasValue)
                    return JsonSerializer.Deserialize<List<Event>>(cache.ToString()!) ?? new List<Event>();
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка чтения ключа {key} из Redis при получении события");
            }

            var events = await _eventRepository.GetTop10Async(ct);

            // Кладем в кэш
            try
            {
                var serialized = JsonSerializer.Serialize(events);
                await _redis.StringSetAsync(key, serialized, ExpireTime);
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, $"Ошибка записи ключа {key} из Redis при получении события");
            }

            return events;
        }
    }
}
