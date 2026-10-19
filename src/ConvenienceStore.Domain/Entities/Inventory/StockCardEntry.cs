using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Inventory;

/// <summary>
/// Thẻ kho bất biến ghi vết mọi biến động xuất/nhập/kiểm kê/hủy
/// </summary>
public class StockCardEntry : AuditableEntity<Guid>
{
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public StockTransactionType TransactionType { get; private set; }
    public string ReferenceCode { get; private set; } = null!;
    public decimal BalanceBefore { get; private set; }
    public decimal QuantityChange { get; private set; }
    public decimal BalanceAfter { get; private set; }
    public string? Note { get; private set; }

    private StockCardEntry() { }
    
    public static StockCardEntry Create(
        Guid storeId,
        Guid productId,
        StockTransactionType transactionType,
        string referenceCode,
        decimal balanceBefore,
        decimal quantityChange,
        decimal balanceAfter,
        string? note = null
    )
    {
        return new StockCardEntry
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ProductId = productId,
            TransactionType = transactionType,
            ReferenceCode = referenceCode.Trim(),
            BalanceBefore = balanceBefore,
            QuantityChange = quantityChange,
            BalanceAfter = balanceAfter,
            Note = note?.Trim()
        };
    }
}
