using Bookings.Application.Repositories;
using Bookings.Domain.Models;
using Events.Application.Repositories;
using Events.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookings.Application.Services
{
    public class BookingBackgroundService : BackgroundService
    {
        //private readonly IBookingRepository _bookingRepository;
        //private readonly IBookingTaskQueue _taskQueue;
        private readonly CancellationToken cancellationToken;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;

        public BookingBackgroundService(
            ILogger<BookingBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory
            //IBookingTaskQueue taskQueue,
            )
        {
            //_taskQueue = taskQueue;
            _logger = logger;
            _scopeFactory = serviceScopeFactory;
            cancellationToken = new CancellationTokenSource().Token;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        var pendingBookings = await bookingRepository.GetAllPendingAsync(stoppingToken);
                        if (pendingBookings.Any())
                        {
                            await FindPendingBookingsAsync(pendingBookings, stoppingToken);
                            await bookingRepository.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка обработки брони");
                }
                await Task.Delay(10000, stoppingToken);
            }
            _logger.LogInformation("BookingBackgroundService остановлен");
        }

        private async Task FindPendingBookingsAsync(IEnumerable<Booking> pendingBookings,
            CancellationToken cancellationToken)
        {
            var tasks = pendingBookings
                .Select(booking => ProcessBookingAsync(booking, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task ProcessBookingAsync(Booking pendingBooking, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            try
            {
                _logger.LogInformation($"Начата обработка брони {pendingBooking.Id}");
                await Task.Delay(2000, cancellationToken);

                using (var scope = _scopeFactory.CreateScope())
                {
                    //var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    //var eventForBook = await context.Events.FirstOrDefaultAsync(e => e.Id == pendingBooking.EventId,
                    //    cancellationToken);
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                    Event? eventForBook = null;
                    eventForBook = await eventRepository.GetByIdAsync(pendingBooking.EventId, cancellationToken);

                    if (eventForBook is null)
                    {
                        pendingBooking.Status = BookingStatus.Rejected;
                        _logger.LogWarning($"Событие {pendingBooking.EventId} отсутствует в хранилище");
                    }
                    else
                    {
                        pendingBooking.Status = BookingStatus.Confirmed;
                        pendingBooking.ProcessedAt = DateTime.UtcNow;

                        _logger.LogInformation($"Бронь {pendingBooking.Id} обработана");
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch
            {
                _logger.LogInformation($"Бронь {pendingBooking.Id} отклонена из-за ошибки");

                pendingBooking.Status = BookingStatus.Rejected;
            }
            finally
            {
            }
        }
    }
}
