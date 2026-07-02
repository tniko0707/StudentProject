using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class User
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Login { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public List<Booking> Bookings { get; set; }

    }
}
