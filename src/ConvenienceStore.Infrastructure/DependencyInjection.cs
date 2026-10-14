using ConvenienceStore.Infrastructure.Persistence;
using ConvenienceStore.Infrastructure.Persistence.Interceptors;
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
        // Đăng ký Interceptor
        services.AddScoped<AuditableEntityInterceptor>();

        // Đăng ký DbContext với PostgreSQL Npgsql
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
