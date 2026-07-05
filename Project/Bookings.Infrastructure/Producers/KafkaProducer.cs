using Bookings.Application.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Shared.Contracts;
using Shared.Contracts.Messages;
using System.Text.Json;

namespace Bookings.Infrastructure.Producers
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(ProducerConfig producerConfig, ILogger<KafkaProducer> logger)
        {
            _producerConfig = producerConfig;
            _logger = logger;
        }

        public async Task PublishBookingConfirmedAsync(BookingConfirmedEvent message, CancellationToken ct = default)
        {
            using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();

            var kafkaMessage = new Message<string, string>
            {
                Key = message.EventId.ToString(),
                Value = JsonSerializer.Serialize(message)
            };

            await producer.ProduceAsync(Topics.BookingConfirmed, kafkaMessage, ct);

            _logger.LogInformation("Отправка сообщения в kafka");
        }
    }
}
