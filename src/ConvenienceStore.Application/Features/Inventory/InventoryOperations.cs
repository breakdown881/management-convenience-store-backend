using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Inventory;
using ConvenienceStore.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Inventory;

// 1. Nhập hàng vào kho chi nhánh (Inbound / Khởi tạo tồn)
public record InboundInventoryCommand(
    Guid StoreId,
    Guid ProductId,
    decimal QuantityInBaseUnit,
    string ReferenceCode,
    string? Note
) : IRequest<Result<decimal>>;

// 2. Kiểm kê kho (Cycle Count / Stocktake) và tự động cân bằng kho
public record ExecuteStocktakeCommand(
    Guid StoreId,
    Guid ProductId,
    decimal ActualCountedQuantity,
    string? Note
) : IRequest<Result<StocktakeResultDto>>;

public record StocktakeResultDto(
    decimal SystemQuantityBefore,
    decimal ActualCountedQuantity,
    decimal VarianceQuantity,
    string AdjustmentReferenceCode
);

// 3. Lập phiếu & Duyệt phiếu xuất hủy hàng hỏng (Write-off / Scrap)
public record CreateWriteOffCommand(
    Guid StoreId,
    Guid ProductId,
    decimal Quantity,
    string Reason
) : IRequest<Result<Guid>>;

public record ApproveWriteOffCommand(Guid WriteOffId) : IRequest<Result>;

// 4. Truy vấn Tồn kho & Thẻ kho
public record StoreInventoryDto(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal OnHandQuantity,
    decimal ReservedQuantity,
    decimal AvailableQuantity
);

public record GetStoreInventoryQuery(Guid StoreId) : IRequest<Result<List<StoreInventoryDto>>>;

public record StockCardDto(
    Guid Id,
    StockTransactionType TransactionType,
    string ReferenceCode,
    decimal BalanceBefore,
    decimal QuantityChange,
    decimal BalanceAfter,
    string? Note,
    DateTime CreatedOnUtc
);

public record GetStockCardQuery(Guid StoreId, Guid ProductId) : IRequest<Result<List<StockCardDto>>>;

public class InventoryCommandHandlers:
    IRequestHandler<InboundInventoryCommand, Result<decimal>>,
    IRequestHandler<ExecuteStocktakeCommand, Result<StocktakeResultDto>>,
    IRequestHandler<CreateWriteOffCommand, Result<Guid>>,
    IRequestHandler<ApproveWriteOffCommand, Result>,
    IRequestHandler<GetStoreInventoryQuery, Result<List<StoreInventoryDto>>>,
    IRequestHandler<GetStockCardQuery, Result<List<StockCardDto>>>
{
    private readonly IAppDbContext _dbcontext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public InventoryCommandHandlers(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditService auditService
    )
    {
        _dbcontext = dbContext;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<decimal>> Handle(InboundInventoryCommand request, CancellationToken cancellationToken)
    {
        var inv = await _dbcontext.StoreInventories
            .FirstOrDefaultAsync(x => x.StoreId == request.StoreId && x.ProductId == request.ProductId, cancellationToken);

        if (inv == null)
        {
            inv = StoreInventory.Create(request.StoreId, request.ProductId, 0);
            _dbcontext.StoreInventories.Add(inv);
        }

        var (before, after) = inv.AddStock(request.QuantityInBaseUnit);

        _dbcontext.StockCardEntries.Add(StockCardEntry.Create(
            request.StoreId,
            request.ProductId,
            StockTransactionType.InitiaOrInbound,
            request.ReferenceCode,
            before,
            request.QuantityInBaseUnit,
            after,
            request.Note
        ));

        await _dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(after);
    }

    public async Task<Result<StocktakeResultDto>> Handle(ExecuteStocktakeCommand request, CancellationToken cancellationToken)
    {
        var inv = await _dbcontext.StoreInventories
            .FirstOrDefaultAsync(x => x.StoreId == request.StoreId && x.ProductId == request.ProductId, cancellationToken);

        if (inv == null)
        {
            inv = StoreInventory.Create(request.StoreId, request.ProductId, 0);
            _dbcontext.StoreInventories.Add(inv);
        }

        var (before, after, variance) = inv.ApplyStocktakeCount(request.ActualCountedQuantity);
        var refCode = $"ST-{DateTime.UtcNow:yyyyMMddHHmmss}";

        _dbcontext.StockCardEntries.Add(StockCardEntry.Create(
            request.StoreId,
            request.ProductId,
            StockTransactionType.StocktakeAdjustment,
            refCode,
            before,
            variance,
            after,
            request.Note ?? "Cân bằng kho tự động qua kiểm kê"
        ));

        await _dbcontext.SaveChangesAsync(cancellationToken);

        await _auditService.LogSensitiveActionAsync(
            "INVENTORY_STOCKTAKE_ADJUST",
            "StoreInventory",
            inv.Id.ToString(),
            new { OnHandBefore = before },
            new { OnHandAfter = after, Variance = variance, ReferenceCode = refCode },
            cancellationToken
        );

        return Result.Success(new StocktakeResultDto(before, after, variance, refCode));
    }

    public async Task<Result<Guid>> Handle(CreateWriteOffCommand request, CancellationToken cancellationToken)
    {
        var writeOff = StockWriteOff.Create(request.StoreId, request.ProductId, request.Quantity, request.Reason);
        _dbcontext.StockWriteOffs.Add(writeOff);
        await _dbcontext.SaveChangesAsync(cancellationToken);
        return Result.Success(writeOff.Id);
    }

    public async Task<Result> Handle(ApproveWriteOffCommand request, CancellationToken cancellationToken)
    {
        var writeOff = await _dbcontext.StockWriteOffs
            .FirstOrDefaultAsync(w => w.Id == request.WriteOffId, cancellationToken);

        if (writeOff == null)
            return Result.Failure(Error.NotFound("WriteOff.NotFound", "Không tìm thấy phiếu xuất hủy."));

        if (writeOff.Status != WriteOffStatus.PendingApproval)
            return Result.Failure(Error.Conflict("WriteOff.AlreadyProcessed", "Phiếu xuất hủy đã được xử lý trước đó."));

        var inv = await _dbcontext.StoreInventories
            .FirstOrDefaultAsync(x => x.StoreId == writeOff.StoreId && x.ProductId == writeOff.ProductId, cancellationToken);

        if (inv == null || inv.AvailableQuantity < writeOff.Quantity)
            return Result.Failure(Error.Failure("Inventory.Insufficient", "Tồn kho không đủ để duyệt xuất hủy."));

        Guid.TryParse(_currentUserService.UserId, out var managerId);
        writeOff.Approve(managerId);

        var (before, after) = inv.DeductStock(writeOff.Quantity);

        _dbcontext.StockCardEntries.Add(StockCardEntry.Create(
            writeOff.StoreId,
            writeOff.ProductId,
            StockTransactionType.WriteOffScrap,
            writeOff.Code,
            before,
            -writeOff.Quantity,
            after,
            $"Xuất hủy: {writeOff.Reason}"
        ));

        await _dbcontext.SaveChangesAsync(cancellationToken);

        await _auditService.LogSensitiveActionAsync(
            "INVENTORY_WRITEOFF_APPROVED",
            "StockWriteOff",
            writeOff.Id.ToString(),
            new { Status = "PendingApproval", OnHandBefore = before },
            new { Status = "Approved", QuantityScrapped = writeOff.Quantity, OnHandAfter = after, Reason = writeOff.Reason },
            cancellationToken
        );

        return Result.Success();
    }

    public async Task<Result<List<StoreInventoryDto>>> Handle(GetStoreInventoryQuery request, CancellationToken cancellationToken)
    {
        var items = await _dbcontext.StoreInventories
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.StoreId == request.StoreId)
            .Select(x => new StoreInventoryDto(
                x.ProductId,
                x.Product.Sku,
                x.Product.Name,
                x.OnHandQuantity,
                x.ReservedQuantity,
                x.OnHandQuantity - x.ReservedQuantity
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }

    public async Task<Result<List<StockCardDto>>> Handle(GetStockCardQuery request, CancellationToken cancellationToken)
    {
        var cards = await _dbcontext.StockCardEntries
            .AsNoTracking()
            .Where(x => x.StoreId == request.StoreId && x.ProductId == request.ProductId)
            .OrderByDescending(x => x.CreatedOnUtc)
            .Select(x => new StockCardDto(
                x.Id,
                x.TransactionType,
                x.ReferenceCode,
                x.BalanceBefore,
                x.QuantityChange,
                x.BalanceAfter,
                x.Note,
                x.CreatedOnUtc
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(cards);
    }
}