using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title).IsRequired().HasMaxLength(200);

            builder.Property(e => e.Description).HasMaxLength(200);

            builder.Property(e => e.StartAt).IsRequired();
            builder.Property(e => e.EndAt).IsRequired();
            builder.Property(e => e.TotalSeats).IsRequired();
            builder.Property(e => e.AvailableSeats).IsRequired();

            builder.HasMany(b => b.Bookings).WithOne(u => u.Event)
                .HasForeignKey(k => k.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
