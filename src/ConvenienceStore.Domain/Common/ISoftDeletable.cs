namespace ConvenienceStore.Domain.Common;

/// <summary>
/// Giao diện cho các thực thể áp dụng cơ chế Xóa Mềm (Soft Delete).
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedOnUtc { get; set; }
    string? DeletedBy { get; set; }
}