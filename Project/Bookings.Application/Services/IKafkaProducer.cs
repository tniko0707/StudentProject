using Shared.Contracts.Messages;

namespace Bookings.Application.Services
{
    public interface IKafkaProducer
    {
        Task PublishBookingConfirmedAsync(BookingConfirmedEvent message, CancellationToken ct = default);
    }
}
