namespace ConvenienceStore.Domain.Entities.Audit;

/// <summary>
/// Đại diện cho bản ghi kiểm toán bất biến ghi nhận mọi thao tác nhạy cảm trong hệ thống
/// </summary>
public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityName { get; private set; } = null!;
    public string? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime TimestampUtc { get; private set; }

    private AuditLog() { } // Dành riêng cho EF Core

    public static AuditLog Create(
        Guid? userId,
        string? username,
        string action,
        string entityName,
        string? entityId,
        string? oldvalues,
        string? newValues,
        string? ipAddress,
        string? userAgent
    )
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Username = username,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValues = oldvalues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            TimestampUtc = DateTime.UtcNow
        };
    }
}
