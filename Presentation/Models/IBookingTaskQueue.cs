using Bookings.Domain.Models;


namespace Presentation.Models
{
    public interface IBookingTaskQueue
    {
        void Enqueue(BookingTask bookingTask);
        bool TryDequeue(out BookingTask bookingTask);
    }
}
