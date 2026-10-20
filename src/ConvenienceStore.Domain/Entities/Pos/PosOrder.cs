using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Pos;

public class PosOrder : AuditableEntity<Guid>, IAggregateRoot
{
    public string OrderNumber { get; private set; } = null!;
    public Guid StoreId { get; private set; }
    public Guid ShiftId { get; private set; }
    public Guid CashierUserId { get; private set; }
    public PosOrderStatus Status { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal TotalVatAmount { get; private set; }
    public decimal GrandTotal { get; private set; }
    public decimal AmountTendered { get; private set; }
    public decimal ChangeAmount { get; private set; }

    public string? Note { get; private set; }
    public string? VoidOrReturnReason { get; private set; }
    public Guid? AuthorizedManagerId { get; private set; }

    private readonly List<PosOrderItem> _items = new();
    public IReadOnlyCollection<PosOrderItem> Items => _items.AsReadOnly();

    private PosOrder() { }

    public static PosOrder Create(
        Guid storeId,
        Guid shiftId,
        Guid cashierUserId,
        PosOrderStatus initialStatus,
        string? note = null
    )
    {
        return new PosOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            StoreId = storeId,
            ShiftId = shiftId,
            CashierUserId = cashierUserId,
            Status = initialStatus,
            Note = note?.Trim()
        };
    }

    public void AddItem(
        Guid productId,
        string sku,
        string productName,
        Guid unitOfMeasureId,
        decimal quantity,
        decimal conversionFactor,
        decimal unitPrice,
        int vatPercent
    )
    {
        var item = PosOrderItem.Create(
            Id,
            productId,
            sku,
            productName,
            unitOfMeasureId,
            quantity,
            conversionFactor,
            unitPrice,
            vatPercent
        );

        _items.Add(item);
        RecalculateTotals();
    }

    public void ClearItems()
    {
        _items.Clear();
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.LineSubTotal);
        TotalVatAmount = _items.Sum(i => i.LineVatAmount);
        GrandTotal = _items.Sum(i => i.LineTotal);
    }

    public void CompletePayment(PaymentMethod method, decimal amountTendered)
    {
        PaymentMethod = method;
        AmountTendered = amountTendered;
        ChangeAmount = method == Enums.PaymentMethod.Cash ? Math.Max(0, amountTendered - GrandTotal) : 0;
        Status = PosOrderStatus.Completed;
    }

    public void VoidOrder(string reason, Guid managerId)
    {
        Status = PosOrderStatus.Voided;
        VoidOrReturnReason = reason.Trim();
        AuthorizedManagerId = managerId;
    }

    public void ReturnOrder(string reason, Guid managerId)
    {
        Status = PosOrderStatus.Returned;
        VoidOrReturnReason = reason.Trim();
        AuthorizedManagerId = managerId;
    }
}

public class PosOrderItem : AuditableEntity<Guid>
{
    public Guid PosOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public Guid UnitOfMeasureId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal ConversionFactor { get; private set; }
    public decimal BaseUnitQuantity => Quantity * ConversionFactor;

    public decimal UnitPrice { get; private set; }
    public int VatPercent { get; private set; }
    public decimal LineSubTotal { get; private set; }
    public decimal LineVatAmount { get; private set; }
    public decimal LineTotal { get; private set; }

    private PosOrderItem() { }
    
    public static PosOrderItem Create(
        Guid posOrderId,
        Guid productId,
        string sku,
        string productName,
        Guid unitOfMeasureId,
        decimal quantity,
        decimal conversionFactor,
        decimal unitPrice,
        int vatPercent
    )
    {
        var subTotal = Math.Round(quantity * unitPrice, 0);
        var vatAmount = Math.Round(subTotal * (vatPercent / 100m), 0);

        return new PosOrderItem
        {
            Id = Guid.NewGuid(),
            PosOrderId = posOrderId,
            ProductId = productId,
            Sku = sku,
            ProductName = productName,
            UnitOfMeasureId = unitOfMeasureId,
            Quantity = quantity,
            ConversionFactor = conversionFactor,
            UnitPrice = unitPrice,
            VatPercent = vatPercent,
            LineSubTotal = subTotal,
            LineVatAmount = vatAmount,
            LineTotal = subTotal + vatAmount
        };
    }
}
