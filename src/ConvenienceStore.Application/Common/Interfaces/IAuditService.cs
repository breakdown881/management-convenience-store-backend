namespace ConvenienceStore.Application.Common.Interfaces;

public interface IAuditService
{
    /// <summary>
    /// Ghi nhận một thao tác nhạy cảm vào hệ thống Audit Trail
    /// </summary>
    Task LogSensitiveActionAsync(
        string action,
        string entityName,
        string? entityId,
        object? oldValues = null,
        object? newValues = null,
        CancellationToken cancellationToken = default
    );
}
