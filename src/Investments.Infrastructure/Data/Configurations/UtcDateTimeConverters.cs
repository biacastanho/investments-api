using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Investments.Infrastructure.Data.Configurations;

/// <summary>
/// O SQL Server não guarda o Kind do DateTime; sem isso os valores lidos voltam como
/// Unspecified e o JSON sai sem o sufixo "Z". Estes conversores gravam e leem sempre em UTC.
/// </summary>
internal static class UtcDateTimeConverters
{
    public static readonly ValueConverter<DateTime, DateTime> Utc = new(
        v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    public static readonly ValueConverter<DateTime?, DateTime?> NullableUtc = new(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : v.Value.ToUniversalTime()) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
}
