using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Infrastructure.Persistence;
using ConvenienceStore.Infrastructure.Persistence.Interceptors;
using ConvenienceStore.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConvenienceStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 1. Đăng ký Security Services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // 2. Đăng ký Interceptor
        services.AddScoped<AuditableEntityInterceptor>();

        // 3. Đăng ký DbContext với PostgreSQL Npgsql
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}
