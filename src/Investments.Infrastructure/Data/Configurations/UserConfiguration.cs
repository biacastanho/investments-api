using Investments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investments.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
        builder.Property(u => u.CreatedAt).HasConversion(UtcDateTimeConverters.Utc).IsRequired();

        builder.HasMany(u => u.Investments)
            .WithOne(i => i.User)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
