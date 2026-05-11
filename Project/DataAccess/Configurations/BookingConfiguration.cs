using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Models;

namespace Project.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(e => e.Id);

            builder.Property(b => b.Status).IsRequired()
                .HasConversion(
                    v => v.ToString(),                    // преобразование enum → строка
                    v => (BookingStatus)Enum.Parse(typeof(BookingStatus), v) // преобразование строки → enum
                )
                .HasMaxLength(50);
                
            builder.Property(b => b.EventId).IsRequired();
            builder.Property(b => b.CreatedAt).IsRequired();

            builder.HasOne(b => b.Event).WithMany(e => e.Bookings)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
