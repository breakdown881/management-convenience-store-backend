using System.Reflection;
using ConvenienceStore.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly AuditableEntityInterceptor _auditableInterceptor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor
    ) : base(options)
    {
        _auditableInterceptor = auditableEntityInterceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
