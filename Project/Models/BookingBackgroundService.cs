namespace Project.Models
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingTaskQueue _taskQueue;
        private readonly ILogger<BookingBackgroundService> _logger;

        public BookingBackgroundService(IBookingTaskQueue taskQueue, 
            ILogger<BookingBackgroundService> logger, 
            IBookingRepository bookingRepository)
        {
            _taskQueue = taskQueue;
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
                    if (_taskQueue.TryDequeue(out BookingTask bookingTask))
                    {
                        _logger.LogInformation($"Начата обработка брони {bookingTask.Id}");

                        await Task.Delay(2000, stoppingToken);

                        var booking = await _bookingRepository.FindByIdAsync(bookingTask.Id);
                        if (booking != null) 
                        { 
                            booking.Status = BookingStatus.Confirmed;
                            booking.ProcessedAt = DateTime.Now;
                        }

                        _logger.LogInformation($"Бронь {bookingTask.Id} обработана");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка обработки брони");
                }
                await Task.Delay(10000, stoppingToken);
            }

            _logger.LogInformation("BookingBackgroundService остановлен");
        }
    }
}
