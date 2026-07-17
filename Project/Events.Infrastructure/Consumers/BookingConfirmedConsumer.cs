using Confluent.Kafka;
using Events.Application.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.Messages;
using System.Text.Json;

namespace Events.Infrastructure.Consumers
{
    public class BookingConfirmedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingConfirmedConsumer> _logger;
        private readonly IEventCacheRepository _eventCacheRepository;
        private readonly ConsumerConfig _consumerConfig;

        public BookingConfirmedConsumer(IServiceScopeFactory scopeFactory, 
            ILogger<BookingConfirmedConsumer> logger, 
            IEventCacheRepository eventCacheRepository,
            ConsumerConfig consumerConfig)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _eventCacheRepository = eventCacheRepository;
            _consumerConfig = consumerConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Заработал consumer");
            await Task.Run(() => Consume(stoppingToken), stoppingToken);
        }

        private void Consume(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();

            consumer.Subscribe(Topics.BookingConfirmed);

            _logger.LogInformation($"Consumer запущен. Ожидание сообщений из топика {Topics.BookingConfirmed.ToString()}");


            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = consumer.Consume(stoppingToken);

                    var message = JsonSerializer.Deserialize<BookingConfirmedEvent>(result.Message.Value);

                    using var scope = _scopeFactory.CreateScope();

                    var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

                    HandleAsync(message, repository, stoppingToken).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки в kafka event");
            }

            consumer.Close();
        }

        private async Task HandleAsync(BookingConfirmedEvent? message, 
            IEventRepository repository, 
            CancellationToken stoppingToken)
        {
            var eventt = await repository.GetByIdAsync(message.EventId);
            if (eventt == null)
            {
                _logger.LogError($"Событие: {message.EventId} отсутствует");
                return;
            }

            eventt.TryReserveSeats(message.SeatsNumber);

            await repository.UpdateAsync(eventt);

            await _eventCacheRepository.DeleteKey(message.EventId);

            _logger.LogInformation($"Событие: {eventt.Id}, Заброниро: {message.SeatsNumber}, Доступно: {eventt.AvailableSeats}");
        }
    }
}
