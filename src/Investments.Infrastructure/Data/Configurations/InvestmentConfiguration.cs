using Investments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investments.Infrastructure.Data.Configurations;

public class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> builder)
    {
        builder.ToTable("Investments");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.UserId).IsRequired();
        builder.Property(i => i.InvestmentTypeId).IsRequired();
        builder.Property(i => i.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(i => i.InvestedAt).HasConversion(UtcDateTimeConverters.Utc).IsRequired();
        builder.Property(i => i.Description).HasMaxLength(200);
        builder.Property(i => i.CreatedAt).HasConversion(UtcDateTimeConverters.Utc).IsRequired();
        builder.Property(i => i.UpdatedAt).HasConversion(UtcDateTimeConverters.NullableUtc);
        builder.HasIndex(i => i.UserId);

        // Restrict: um tipo em uso não pode ser apagado levando investimentos junto.
        builder.HasOne(i => i.Type)
            .WithMany(t => t.Investments)
            .HasForeignKey(i => i.InvestmentTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
