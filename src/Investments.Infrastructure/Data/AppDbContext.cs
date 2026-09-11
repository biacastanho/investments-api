using Investments.Application.Exceptions;
using Investments.Domain.Entities;
using Investments.Domain.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Investments.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    // Códigos do SQL Server para violação de índice/constraint única.
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Investment> Investments => Set<Investment>();
    public DbSet<InvestmentType> InvestmentTypes => Set<InvestmentType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        try
        {
            return await SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Corrida entre duas requisições com o mesmo e-mail: vira 409 em vez de 500.
            throw new ConflictException("Já existe um registro cadastrado com estes dados.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sql &&
        sql.Number is UniqueIndexViolation or UniqueConstraintViolation;
}
