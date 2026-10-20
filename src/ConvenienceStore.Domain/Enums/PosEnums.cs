namespace ConvenienceStore.Domain.Enums;

public enum ShiftStatus
{
    Open = 1,
    Closed = 2
}

public enum CashTransactionType
{
    PettyCashIn = 1,
    PettyCashOut = 2,
    SafeDrop = 3
}

public enum PosOrderStatus
{
    Held = 1,
    Completed = 2,
    Voided = 3,
    Returned = 4
}

public enum PaymentMethod
{
    Cash = 1,
    QrTransfer = 2,
    Card = 3
}