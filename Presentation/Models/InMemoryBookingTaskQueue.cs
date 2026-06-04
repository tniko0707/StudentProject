
using Domain.Models;
using System.Collections.Concurrent;

namespace Presentation.Models
{
    public class InMemoryBookingTaskQueue : IBookingTaskQueue
    {
        private readonly ConcurrentQueue<BookingTask> _taskQueue = new();
        public void Enqueue(BookingTask bookingTask)
        {
            _taskQueue.Enqueue(bookingTask);
        }

        public bool TryDequeue(out BookingTask bookingTask)
        {
            return _taskQueue.TryDequeue(out bookingTask);
        }
    }
}
