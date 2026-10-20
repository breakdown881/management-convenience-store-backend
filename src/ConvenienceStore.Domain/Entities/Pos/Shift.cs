using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Enums;

namespace ConvenienceStore.Domain.Entities.Pos;

public class Shift : AuditableEntity<Guid>, IAggregateRoot
{
    public string ShiftCode { get; private set; } = null!;
    public Guid StoreId { get; private set; }
    public Guid PosTerminalId { get; private set; }
    public Guid CashierUserId { get; private set; }
    public DateTime OpenedAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public ShiftStatus Status { get; private set; }

    // Các chỉ số tài chính trong ca
    public decimal OpeningFloat { get; private set; }
    public decimal TotalCashSales { get; private set; }
    public decimal TotalNonCashSales { get; private set; }
    public decimal TotalCashRefunds { get; private set; }
    public decimal TotalPettyCashIn { get; private set; }
    public decimal TotalPettyCashOut { get; private set; }
    public decimal TotalSafeDrops { get; private set; }

    // Kết quả kiểm đếm Blind Drop cuối ca
    public decimal? ExpectedCashAtClose { get; private set; }
    public decimal? ActualCountedCashAtClose { get; private set; }
    public decimal? CashVariance { get; private set; } // Dương: Thừa tiền, Âm: Thiếu tiền

    private readonly List<ShiftCashTransaction> _cashTransactions = new();
    public IReadOnlyCollection<ShiftCashTransaction> CashTransactions => _cashTransactions.AsReadOnly();

    private Shift() { }

    public static Shift Open(Guid storeId, Guid posTerminalId, Guid cashierUserId, decimal openingFloat)
    {
        return new Shift
        {
            Id = Guid.NewGuid(),
            ShiftCode = $"SHF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            StoreId = storeId,
            PosTerminalId = posTerminalId,
            CashierUserId = cashierUserId,
            OpenedAtUtc = DateTime.UtcNow,
            Status = ShiftStatus.Open,
            OpeningFloat = openingFloat
        };
    }

    public void RecordCashTransaction(CashTransactionType type, decimal amount, string reason)
    {
        _cashTransactions.Add(ShiftCashTransaction.Create(Id, type, amount, reason));

        switch (type)
        {
            case CashTransactionType.PettyCashIn:
                TotalPettyCashIn += amount;
                break;


            case CashTransactionType.PettyCashOut:
                TotalPettyCashOut += amount;
                break;

            case CashTransactionType.SafeDrop:
                TotalSafeDrops += amount;
                break;
        }
    }

    public void RecordOrderPayment(PaymentMethod method, decimal totalAmount)
    {
        if (method == PaymentMethod.Cash)
            TotalCashSales += totalAmount;
        else
            TotalNonCashSales += totalAmount;
    }

    public void RecordCashRefund(decimal refundAmount)
    {
        TotalCashRefunds += refundAmount;
    }

    public void CloseShiftBlindDrop(decimal actualCountedCash)
    {
        ExpectedCashAtClose = OpeningFloat + TotalCashSales - TotalCashRefunds + TotalPettyCashIn - TotalPettyCashOut - TotalSafeDrops;
        ActualCountedCashAtClose = actualCountedCash;
        CashVariance = ActualCountedCashAtClose - ExpectedCashAtClose;
        ClosedAtUtc = DateTime.UtcNow;
        Status = ShiftStatus.Closed;
    }
}

public class ShiftCashTransaction : AuditableEntity<Guid>
{
    public Guid ShiftId { get; private set; }
    public CashTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; } = null!;

    private ShiftCashTransaction() { }
    
    public static ShiftCashTransaction Create(Guid shiftId, CashTransactionType type, decimal amount, string reason)
    {
        return new ShiftCashTransaction
        {
            Id = Guid.NewGuid(),
            ShiftId = shiftId,
            Type = type,
            Amount = amount,
            Reason = reason.Trim()
        };
    }
}
