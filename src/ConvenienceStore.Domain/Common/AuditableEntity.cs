namespace ConvenienceStore.Domain.Common;

/// <summary>
/// Lớp cơ sở cho các Entity cần lưu vết Audit Trail.
/// </summary>
public abstract class AuditableEntity<TId> : BaseEntity<TId>
{
    public DateTime CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOnUtc { get; set; }
    public string? ModifiedBy { get; set; }

    protected AuditableEntity(TId id) : base(id)
    {
    }

    protected AuditableEntity()
    {
    }
}