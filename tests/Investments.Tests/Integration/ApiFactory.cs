using Investments.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Investments.Tests.Integration;

/// <summary>Sobe a API em memória trocando o SQL Server por um banco InMemory.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"InvestmentsTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            (d.ServiceType.FullName?.Contains("EntityFrameworkCore") ?? false))
                .ToList();
            foreach (var d in descriptors) services.Remove(d);

            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // EnsureCreated aplica o HasData, sem o qual os tipos de investimento não existiriam.
            using var scope = services.BuildServiceProvider().CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        });
    }
}
