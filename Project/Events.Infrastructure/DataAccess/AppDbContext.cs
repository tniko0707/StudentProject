using Events.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.DataAccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<Event> Events { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
