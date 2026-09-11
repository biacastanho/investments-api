using Investments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Investments.Infrastructure.Data.Configurations;

public class InvestmentTypeConfiguration : IEntityTypeConfiguration<InvestmentType>
{
    /// <summary>Data fixa para o seed: usar DateTime.UtcNow tornaria a migration não determinística.</summary>
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<InvestmentType> builder)
    {
        builder.ToTable("InvestmentTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();
        builder.Property(t => t.Name).HasMaxLength(InvestmentType.MaxNameLength).IsRequired();
        builder.HasIndex(t => t.Name).IsUnique();
        builder.Property(t => t.CreatedAt).HasConversion(UtcDateTimeConverters.Utc).IsRequired();
        builder.Property(t => t.UpdatedAt).HasConversion(UtcDateTimeConverters.NullableUtc);

        // Os cinco tipos que antes eram valores do enum, com os mesmos ids para preservar os dados existentes.
        builder.HasData(
            Seed(1, "Acoes"),
            Seed(2, "RendaFixa"),
            Seed(3, "Fundos"),
            Seed(4, "Tesouro"),
            Seed(5, "Cripto"));
    }

    private static object Seed(int id, string name) =>
        new { Id = id, Name = name, CreatedAt = SeedDate, UpdatedAt = (DateTime?)null };
}
