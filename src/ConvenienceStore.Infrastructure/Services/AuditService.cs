using System.Text.Json;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Entities.Audit;

namespace ConvenienceStore.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService
    )
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task LogSensitiveActionAsync(
        string action,
        string entityName,
        string? entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default
    )
    {
        Guid? userId = null;
        if (Guid.TryParse(_currentUserService.UserId, out var parsedGuid))
        {
            userId = parsedGuid;
        }

        var oldValuesJson = oldValues != null ? JsonSerializer.Serialize(oldValues) : null;
        var newValuesJson = newValues != null ? JsonSerializer.Serialize(newValues) : null;

        var log = AuditLog.Create(
            userId: userId,
            username: _currentUserService.Username,
            action: action,
            entityName: entityName,
            entityId: entityId,
            oldvalues: oldValuesJson,
            newValues: newValuesJson,
            ipAddress: _currentUserService.IpAddress,
            userAgent: _currentUserService.UserAgent
        );

        _dbContext.AuditLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
