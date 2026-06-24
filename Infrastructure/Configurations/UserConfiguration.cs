using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.DataAccess.Configurations
{
    public class UserConfiguration: IEntityTypeConfiguration<User> 
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(e => e.UserId);

            builder.Property(e => e.Role).IsRequired();

            builder.Property(e => e.Login).IsRequired();

            builder.HasIndex(e => e.Login).IsUnique();

            builder.HasMany(b => b.Bookings).WithOne(e => e.User)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
        }
    }
}
