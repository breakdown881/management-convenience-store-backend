using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Infrastructure.Persistence;
using ConvenienceStore.Infrastructure.Persistence.Interceptors;
using ConvenienceStore.Infrastructure.Security;
using ConvenienceStore.Infrastructure.Services;
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

        // Đăng ký Audit Service
        services.AddScoped<IAuditService, AuditService>();

        // Cấu hình Redis Distributed Cache (nếu có cấu hình Redis, ngược lại dùng Memory Distributed Cache dự phòng)
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "ConvenienceStore_";
        });

        services.AddScoped<IBarcodeLookupService, FastBarcodeLookupService>();

        return services;
    }
}
