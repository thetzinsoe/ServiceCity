using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;

namespace ServiceCity.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.StatusFrom)
            .HasConversion<int>();

        builder.Property(n => n.StatusTo)
            .HasConversion<int>();

        builder.HasOne(n => n.Booking)
            .WithMany()
            .HasForeignKey(n => n.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.BookingId, n.CreatedAt })
            .IsDescending(false, true);
    }
}
