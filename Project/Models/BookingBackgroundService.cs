using System.Threading.Tasks;

namespace Project.Models
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IBookingRepository _bookingRepository;
        //private readonly IBookingTaskQueue _taskQueue;
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly IEventService _eventService;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

        public BookingBackgroundService( 
            ILogger<BookingBackgroundService> logger, 
            IBookingRepository bookingRepository,
            IEventService eventService
            //IBookingTaskQueue taskQueue,
            )
        {
            //_taskQueue = taskQueue;
            _logger = logger;
            _bookingRepository = bookingRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    #region Обработка через опрос
                    await FindPendingBookings(stoppingToken);
                    #endregion
                    #region Обработка через очередь
                    //if (_taskQueue.TryDequeue(out BookingTask bookingTask))
                    //{
                    //    _logger.LogInformation($"Начата обработка брони {bookingTask.Id}");

                    //    await Task.Delay(2000, stoppingToken);

                    //    var booking = await _bookingRepository.FindByIdAsync(bookingTask.Id);
                    //    if (booking != null) 
                    //    { 
                    //        booking.Status = BookingStatus.Confirmed;
                    //        booking.ProcessedAt = DateTime.Now;
                    //    }

                    //    _logger.LogInformation($"Бронь {bookingTask.Id} обработана");
                    //}
                    #endregion
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка обработки брони");
                }
                await Task.Delay(10000, stoppingToken);
            }

            _logger.LogInformation("BookingBackgroundService остановлен");
        }

        private async Task FindPendingBookings(CancellationToken cancellationToken)
        {
            var pendingBookings = await _bookingRepository.GetAllPendingAsync();
            //foreach (var pendingBooking in pendingBookings)
            //{
            //    _logger.LogInformation($"Начата обработка брони {pendingBooking.Id}");
            //    await Task.Delay(2000, cancellationToken);
            //    pendingBooking.Status = BookingStatus.Confirmed;
            //    pendingBooking.ProcessedAt = DateTime.Now;

            //    _logger.LogInformation($"Бронь {pendingBooking.Id} обработана");
            //}
            var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task<Booking> ProcessBookingAsync(Booking pendingBooking, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            await _processingSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation($"Начата обработка брони {pendingBooking.Id}");
                await Task.Delay(2000, cancellationToken);
                if (_eventService.GetEventById(pendingBooking.EventId) == null)
                {
                    pendingBooking.Status = BookingStatus.Rejected;
                    _logger.LogWarning($"Событие {pendingBooking.EventId} отсутствует в хранилище");
                }
                else
                {
                    pendingBooking.Status = BookingStatus.Confirmed;
                    pendingBooking.ProcessedAt = DateTime.Now;

                    _logger.LogInformation($"Бронь {pendingBooking.Id} обработана");
                }
            }
            catch
            {
                pendingBooking.Status = BookingStatus.Rejected;
            }
            finally
            {
                _processingSemaphore.Release();
            }
            return pendingBooking;
        }
    }
}
