using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceCity.Core.Entities;

namespace ServiceCity.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.PhoneNumberNormalized)
            .HasMaxLength(20);

        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique(false);

        builder.HasIndex(u => u.PhoneNumberNormalized)
            .IsUnique(false);
    }
}
