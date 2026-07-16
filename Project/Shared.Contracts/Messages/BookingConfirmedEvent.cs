using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Contracts.Messages
{
    public class BookingConfirmedEvent
    {
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public Guid BookingId { get; set; }
        public int SeatsNumber { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.Now;

    }
}
