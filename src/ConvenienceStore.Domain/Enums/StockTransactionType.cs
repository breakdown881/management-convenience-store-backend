namespace ConvenienceStore.Domain.Enums;

public enum StockTransactionType
{
    InitiaOrInbound = 1,
    PosSaleOut = 2,
    PosReturnIn = 3,
    StocktakeAdjustment = 4,
    WriteOffScrap = 5
}

public enum WriteOffStatus
{
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3
}
