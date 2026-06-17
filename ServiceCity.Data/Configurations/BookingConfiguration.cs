using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCity.Core.Entities;
using ServiceCity.Core.Enums;

namespace ServiceCity.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(12);

        builder.HasIndex(b => b.ReferenceNumber)
            .IsUnique();

        builder.Property(b => b.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.Status)
            .HasConversion<int>();

        builder.Property(b => b.PreferredTimeSlot)
            .HasConversion<int>();

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.ServiceCategory)
            .WithMany()
            .HasForeignKey(b => b.ServiceCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.Status, b.CreatedAt })
            .IsDescending(false, true);

        builder.HasIndex(b => new { b.UserId, b.Status });
    }
}
