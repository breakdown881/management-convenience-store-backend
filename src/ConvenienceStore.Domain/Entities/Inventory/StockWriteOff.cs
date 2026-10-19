using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Inventory;

/// <summary>
/// Phiếu xuất hủy hàng hư hỏng/vỡ/hết hạn cần Store Manager phê duyệt
/// </summary>
public class StockWriteOff : AuditableEntity<Guid>
{
    public string Code { get; private set; } = null!;
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public string Reason { get; private set; } = null!;
    public WriteOffStatus Status { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }

    private StockWriteOff() { }

    public static StockWriteOff Create(Guid storeId, Guid productId, decimal quantity, string reason)
    {
        return new StockWriteOff
        {
            Id = Guid.NewGuid(),
            Code = $"WO-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            StoreId = storeId,
            ProductId = productId,
            Quantity = quantity,
            Reason = reason.Trim(),
            Status = WriteOffStatus.PendingApproval
        };
    }
    
    public void Approve(Guid managerUserId)
    {
        Status = WriteOffStatus.Approved;
        ApprovedByUserId = managerUserId;
        ApprovedAtUtc = DateTime.UtcNow;
    }
}
