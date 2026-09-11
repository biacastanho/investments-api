using Investments.Application.Interfaces;
using Investments.Application.Services;
using Investments.Domain.Repositories;
using Investments.Infrastructure.Data;
using Investments.Infrastructure.Repositories;
using Investments.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Investments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(3)));

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInvestmentRepository, InvestmentRepository>();
        services.AddScoped<IInvestmentTypeRepository, InvestmentTypeRepository>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<IInvestmentTypeService, InvestmentTypeService>();

        return services;
    }
}
