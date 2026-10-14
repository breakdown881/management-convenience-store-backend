using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ConvenienceStore.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var userId = _currentUserService.UserId ?? "System";
        var utcNow = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if(entry.Entity is AuditableEntity<Guid> auditableGuid)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableGuid.CreatedOnUtc = utcNow;
                    auditableGuid.CreatedBy = userId;
                }
                else if(entry.State == EntityState.Modified)
                {
                    auditableGuid.ModifiedOnUtc = utcNow;
                    auditableGuid.ModifiedBy = userId;
                }
            }
        }
    }
}
