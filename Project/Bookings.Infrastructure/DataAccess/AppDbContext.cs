using Bookings.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<Booking> Bookings { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
