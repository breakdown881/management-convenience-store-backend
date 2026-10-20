using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Common;
using ConvenienceStore.Domain.Entities.Inventory;
using ConvenienceStore.Domain.Entities.Pos;
using ConvenienceStore.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ConvenienceStore.Application.Features.Pos;

// 1. Mở Ca làm việc
public record OpenShiftCommand(
    Guid StoreId,
    Guid PosTerminalId,
    decimal OpeningFloat
) : IRequest<Result<Guid>>;

// 2. Phiếu Thu/Chi tại quầy & Rút tiền vào két an toàn (Petty Cash / Safe Drop)
public record CreateShiftCashTransactionCommand(
    Guid ShiftId,
    CashTransactionType Type,
    decimal Amount,
    string Reason
) : IRequest<Result>;

// 3. Đóng ca Blind Drop theo mệnh giá tiền & xuất Z-Report
public record CashDenominationInput(int DenominationValue, int SheetCount);

public record CloseShiftBlindDropCommand(
    Guid ShiftId,
    List<CashDenominationInput> Denominations
) : IRequest<Result<ZReportDto>>;

public record ZReportDto(
    Guid ShiftId,
    string ShiftCode,
    DateTime OpenedAtUtc,
    DateTime ClosedAtUtc,
    decimal OpeningFloat,
    decimal TotalCashSales,
    decimal TotalNonCashSales,
    decimal TotalCashRefunds,
    decimal TotalPettyCashIn,
    decimal TotalPettyCashOut,
    decimal TotalSafeDrops,
    decimal ExpectedCashInDrawer,
    decimal ActualCountedCash,
    decimal CashVariance,
    string VarianceAssessment
);

// 4. Xử lý Đơn hàng POS: Hold Bill / Checkout Bill
public record PosOrderItemInput(
    Guid ProductId,
    Guid UnitOfMeasureId,
    decimal Quantity
);

public record CreateOrCheckoutPosOrderCommand(
    Guid StoreId,
    Guid ShiftId,
    bool IsHoldBill,
    PaymentMethod PaymentMethod,
    decimal AmountTendered,
    string? Note,
    List<PosOrderItemInput> Items
) : IRequest<Result<PosOrderReceiptDto>>;

public record PosOrderReceiptDto(
    Guid OrderId,
    string OrderNumber,
    PosOrderStatus Status,
    decimal SubTotal,
    decimal TotalVatAmount,
    decimal GrandTotal,
    decimal AmountTendered,
    decimal ChangeAmount
);

// 5. Recall Hold Bill & Thanh toán đơn đã Hold
public record RecallAndCheckoutHeldOrderCommand(
    Guid OrderId,
    PaymentMethod PaymentMethod,
    decimal AmountTendered) : IRequest<Result<PosOrderReceiptDto>>;

// 6. Hủy đơn (Void Bill) & Đổi/Trả hàng (Return & Refund) yêu cầu mã PIN Quản lý
public record VoidOrReturnOrderCommand(
    Guid OrderId,
    bool IsReturnRefund,
    string Reason,
    string ManagerPin) : IRequest<Result>;

public class PosAndShiftHandlers :
    IRequestHandler<OpenShiftCommand, Result<Guid>>,
    IRequestHandler<CreateShiftCashTransactionCommand, Result>,
    IRequestHandler<CloseShiftBlindDropCommand, Result<ZReportDto>>,
    IRequestHandler<CreateOrCheckoutPosOrderCommand, Result<PosOrderReceiptDto>>,
    IRequestHandler<RecallAndCheckoutHeldOrderCommand, Result<PosOrderReceiptDto>>,
    IRequestHandler<VoidOrReturnOrderCommand, Result>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    // Mã PIN phê duyệt tại quầy của Store Manager (Mặc định cấu hình mẫu: "999999")
    private const string DefaultManagerApprovalPin = "999999";

    public PosAndShiftHandlers(
        IAppDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> Handle(OpenShiftCommand request, CancellationToken cancellationToken)
    {
        var hasOpenShift = await _dbContext.Shifts
            .AnyAsync(s => s.PosTerminalId == request.PosTerminalId && s.Status == ShiftStatus.Open, cancellationToken);

        if (hasOpenShift)
            return Result.Failure<Guid>(Error.Conflict("Shift.AlreadyOpen", "Máy POS này đang có một ca làm việc chưa đóng."));

        Guid.TryParse(_currentUserService.UserId, out var cashierId);
        var shift = Shift.Open(request.StoreId, request.PosTerminalId, cashierId, request.OpeningFloat);

        _dbContext.Shifts.Add(shift);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(shift.Id);
    }

    public async Task<Result> Handle(CreateShiftCashTransactionCommand request, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId && s.Status == ShiftStatus.Open, cancellationToken);

        if (shift == null)
            return Result.Failure(Error.NotFound("Shift.NotFound", "Không tìm thấy ca làm việc đang mở."));

        shift.RecordCashTransaction(request.Type, request.Amount, request.Reason);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<ZReportDto>> Handle(CloseShiftBlindDropCommand request, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId && s.Status == ShiftStatus.Open, cancellationToken);

        if (shift == null)
            return Result.Failure<ZReportDto>(Error.NotFound("Shift.NotFound", "Không tìm thấy ca làm việc đang mở."));

        // Tính tổng tiền thực tế thu ngân kiểm đếm theo từng mệnh giá (Blind Drop)
        decimal actualCounted = request.Denominations.Sum(d => (decimal)d.DenominationValue * d.SheetCount);

        shift.CloseShiftBlindDrop(actualCounted);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var variance = shift.CashVariance ?? 0m;
        var assessment = variance == 0
            ? "Khớp tuyệt đối (Balanced)"
            : variance > 0
                ? $"Thừa tiền mặt (+{variance:N0} VNĐ)"
                : $"Thiếu hụt tiền mặt ({variance:N0} VNĐ)";

        await _auditService.LogSensitiveActionAsync(
            "SHIFT_CLOSED_BLIND_DROP",
            "Shift",
            shift.Id.ToString(),
            new { Status = "Open" },
            new { ExpectedCash = shift.ExpectedCashAtClose, ActualCounted = actualCounted, Variance = variance, Denominations = request.Denominations },
            cancellationToken);

        return Result.Success(new ZReportDto(
            shift.Id,
            shift.ShiftCode,
            shift.OpenedAtUtc,
            shift.ClosedAtUtc!.Value,
            shift.OpeningFloat,
            shift.TotalCashSales,
            shift.TotalNonCashSales,
            shift.TotalCashRefunds,
            shift.TotalPettyCashIn,
            shift.TotalPettyCashOut,
            shift.TotalSafeDrops,
            shift.ExpectedCashAtClose!.Value,
            actualCounted,
            variance,
            assessment));
    }

    public async Task<Result<PosOrderReceiptDto>> Handle(CreateOrCheckoutPosOrderCommand request, CancellationToken cancellationToken)
    {
        var shift = await _dbContext.Shifts
            .FirstOrDefaultAsync(s => s.Id == request.ShiftId && s.Status == ShiftStatus.Open, cancellationToken);

        if (shift == null)
            return Result.Failure<PosOrderReceiptDto>(Error.Failure("Shift.NotOpen", "Phải mở ca làm việc trước khi tạo đơn hàng POS."));

        Guid.TryParse(_currentUserService.UserId, out var cashierId);
        var initialStatus = request.IsHoldBill ? PosOrderStatus.Held : PosOrderStatus.Completed;
        var order = PosOrder.Create(request.StoreId, request.ShiftId, cashierId, initialStatus, request.Note);

        foreach (var itemInput in request.Items)
        {
            var product = await _dbContext.Products
                .Include(p => p.UnitConversions)
                .FirstOrDefaultAsync(p => p.Id == itemInput.ProductId, cancellationToken);

            if (product == null)
                return Result.Failure<PosOrderReceiptDto>(Error.NotFound("Product.NotFound", $"Không tìm thấy sản phẩm {itemInput.ProductId}."));

            decimal conversionFactor = 1m;
            decimal unitPrice = product.StandardPrice;

            if (itemInput.UnitOfMeasureId != product.BaseUnitId)
            {
                var conv = product.UnitConversions.FirstOrDefault(u => u.UnitOfMeasureId == itemInput.UnitOfMeasureId);
                if (conv != null)
                {
                    conversionFactor = conv.ConversionFactor;
                    unitPrice = conv.SalePrice;
                }
            }

            order.AddItem(
                product.Id,
                product.Sku,
                product.Name,
                itemInput.UnitOfMeasureId,
                itemInput.Quantity,
                conversionFactor,
                unitPrice,
                (int)product.VatRate);
        }

        if (!request.IsHoldBill)
        {
            // Kiểm tra và trừ tồn kho tức thời (theo Đơn vị cơ sở BaseUnitQuantity)
            foreach (var item in order.Items)
            {
                var inv = await _dbContext.StoreInventories
                    .FirstOrDefaultAsync(x => x.StoreId == request.StoreId && x.ProductId == item.ProductId, cancellationToken);

                if (inv == null || inv.AvailableQuantity < item.BaseUnitQuantity)
                {
                    return Result.Failure<PosOrderReceiptDto>(Error.Failure(
                        "Inventory.Insufficient",
                        $"Sản phẩm '{item.ProductName}' không đủ tồn kho (Cần: {item.BaseUnitQuantity})."));
                }

                var (before, after) = inv.DeductStock(item.BaseUnitQuantity);

                _dbContext.StockCardEntries.Add(StockCardEntry.Create(
                    request.StoreId,
                    item.ProductId,
                    StockTransactionType.PosSaleOut,
                    order.OrderNumber,
                    before,
                    -item.BaseUnitQuantity,
                    after,
                    "Xuất bán lẻ tại quầy POS"));
            }

            order.CompletePayment(request.PaymentMethod, request.AmountTendered);
            shift.RecordOrderPayment(request.PaymentMethod, order.GrandTotal);
        }

        _dbContext.PosOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new PosOrderReceiptDto(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.SubTotal,
            order.TotalVatAmount,
            order.GrandTotal,
            order.AmountTendered,
            order.ChangeAmount));
    }

    public async Task<Result<PosOrderReceiptDto>> Handle(RecallAndCheckoutHeldOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.PosOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == PosOrderStatus.Held, cancellationToken);

        if (order == null)
            return Result.Failure<PosOrderReceiptDto>(Error.NotFound("Order.HeldNotFound", "Không tìm thấy hóa đơn đang lưu tạm (Hold Bill)."));

        var shift = await _dbContext.Shifts
            .FirstOrDefaultAsync(s => s.Id == order.ShiftId && s.Status == ShiftStatus.Open, cancellationToken);

        if (shift == null)
            return Result.Failure<PosOrderReceiptDto>(Error.Failure("Shift.Closed", "Ca làm việc của đơn hàng này đã đóng."));

        foreach (var item in order.Items)
        {
            var inv = await _dbContext.StoreInventories
                .FirstOrDefaultAsync(x => x.StoreId == order.StoreId && x.ProductId == item.ProductId, cancellationToken);

            if (inv == null || inv.AvailableQuantity < item.BaseUnitQuantity)
                return Result.Failure<PosOrderReceiptDto>(Error.Failure("Inventory.Insufficient", $"Không đủ tồn kho cho '{item.ProductName}'."));

            var (before, after) = inv.DeductStock(item.BaseUnitQuantity);

            _dbContext.StockCardEntries.Add(StockCardEntry.Create(
                order.StoreId,
                item.ProductId,
                StockTransactionType.PosSaleOut,
                order.OrderNumber,
                before,
                -item.BaseUnitQuantity,
                after,
                "Thanh toán từ đơn Hold Bill"));
        }

        order.CompletePayment(request.PaymentMethod, request.AmountTendered);
        shift.RecordOrderPayment(request.PaymentMethod, order.GrandTotal);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new PosOrderReceiptDto(
            order.Id,
            order.OrderNumber,
            order.Status,
            order.SubTotal,
            order.TotalVatAmount,
            order.GrandTotal,
            order.AmountTendered,
            order.ChangeAmount));
    }

    public async Task<Result> Handle(VoidOrReturnOrderCommand request, CancellationToken cancellationToken)
    {
        // Xác thực mã PIN của Cửa hàng trưởng (TASK-120)
        if (request.ManagerPin != DefaultManagerApprovalPin)
        {
            return Result.Failure(Error.Failure("Pos.InvalidManagerPin", "Mã PIN xác thực của Quản lý cửa hàng không chính xác!"));
        }

        var order = await _dbContext.PosOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == PosOrderStatus.Completed, cancellationToken);

        if (order == null)
            return Result.Failure(Error.NotFound("Order.NotFound", "Không tìm thấy đơn hàng đã hoàn tất để hủy/hoàn trả."));

        var shift = await _dbContext.Shifts.FirstOrDefaultAsync(s => s.Id == order.ShiftId, cancellationToken);
        Guid.TryParse(_currentUserService.UserId, out var managerId);

        // Hoàn lại tồn kho & Ghi thẻ kho
        foreach (var item in order.Items)
        {
            var inv = await _dbContext.StoreInventories
                .FirstOrDefaultAsync(x => x.StoreId == order.StoreId && x.ProductId == item.ProductId, cancellationToken);

            if (inv != null)
            {
                var (before, after) = inv.AddStock(item.BaseUnitQuantity);

                _dbContext.StockCardEntries.Add(StockCardEntry.Create(
                    order.StoreId,
                    item.ProductId,
                    StockTransactionType.PosReturnIn,
                    order.OrderNumber,
                    before,
                    item.BaseUnitQuantity,
                    after,
                    request.IsReturnRefund ? $"Khách trả hàng: {request.Reason}" : $"Hủy bill: {request.Reason}"));
            }
        }

        if (order.PaymentMethod == PaymentMethod.Cash && shift != null)
        {
            shift.RecordCashRefund(order.GrandTotal);
        }

        if (request.IsReturnRefund)
            order.ReturnOrder(request.Reason, managerId);
        else
            order.VoidOrder(request.Reason, managerId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.LogSensitiveActionAsync(
            request.IsReturnRefund ? "POS_ORDER_RETURN_REFUND" : "POS_ORDER_VOIDED",
            "PosOrder",
            order.Id.ToString(),
            new { Status = "Completed", GrandTotal = order.GrandTotal },
            new { Status = order.Status.ToString(), Reason = request.Reason, AuthorizedBy = managerId },
            cancellationToken);

        return Result.Success();
    }
}
