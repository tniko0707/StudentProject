using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Models;

namespace Users.Infrastructure.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users_db");

            builder.HasKey(e => e.UserId);

            builder.Property(e => e.Role).IsRequired().HasConversion<string>();

            builder.Property(e => e.Login).IsRequired().HasMaxLength(50);

            builder.HasIndex(e => e.Login).IsUnique();

            builder.Property(e => e.Email).IsRequired().HasMaxLength(50);

            builder.HasIndex(e => e.Email).IsUnique();

            builder.Property(e => e.CreatedAt).IsRequired();

            //builder.HasMany(b => b.Bookings).WithOne(e => e.User)
            //    .HasForeignKey(k => k.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
