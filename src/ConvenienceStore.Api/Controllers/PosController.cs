using ConvenienceStore.Api.Authorization;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Application.Features.Pos;
using ConvenienceStore.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers;

[ApiController]
[Route("api/v1/pos")]
public class PosController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IBarcodeLookupService _barcodeLookupService;

    public PosController(ISender sender, IBarcodeLookupService barcodeLookupService)
    {
        _sender = sender;
        _barcodeLookupService = barcodeLookupService;
    }

    /// <summary>
    /// Tra cứu Barcode tốc độ cao (< 50ms qua Dapper + Redis Cache), hỗ trợ cả Barcode cân điện tử GS1-13
    /// </summary>
    [HasPermission(Permissions.PosCheckout)]
    [HttpGet("barcode-lookup/{barcode}")]
    public async Task<IActionResult> LookupBarcode(string barcode, CancellationToken cancellationToken)
    {
        var result = await _barcodeLookupService.LookupAsync(barcode, cancellationToken);
        if (result == null)
            return NotFound(new { code = "Barcode.NotFound", message = $"Không tìm thấy sản phẩm với mã vạch '{barcode}'." });

        return Ok(result);
    }

    [HasPermission(Permissions.ShiftOpen)]
    [HttpPost("shifts/open")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftCommand command)
    {
        var result = await _sender.Send(command);
        if (result.IsFailure)
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

        return Ok(new { shiftId = result.Value });
    }

    [HasPermission(Permissions.ShiftPettyCash)]
    [HttpPost("shifts/cash-transactions")]
    public async Task<IActionResult> CreateCashTransaction([FromBody] CreateShiftCashTransactionCommand command)
    {
        var result = await _sender.Send(command);
        if (result.IsFailure)
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

        return Ok(new { message = "Đã ghi nhận phiếu thu/chi/rút tiền két an toàn thành công." });
    }

    [HasPermission(Permissions.ShiftClose)]
    [HttpPost("shifts/close-blind-drop")]
    public async Task<IActionResult> CloseShiftBlindDrop([FromBody] CloseShiftBlindDropCommand command)
    {
        var result = await _sender.Send(command);
        if (result.IsFailure)
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

        return Ok(result.Value);
    }

    [HasPermission(Permissions.PosCheckout)]
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrCheckoutOrder([FromBody] CreateOrCheckoutPosOrderCommand command)
    {
        var result = await _sender.Send(command);
        if (result.IsFailure)
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

        return Ok(result.Value);
    }

    [HasPermission(Permissions.PosCheckout)]
    [HttpPost("orders/{orderId:guid}/recall-checkout")]
    public async Task<IActionResult> RecallAndCheckout(Guid orderId, [FromBody] RecallCheckoutRequest request)
    {
        var result = await _sender.Send(new RecallAndCheckoutHeldOrderCommand(orderId, request.PaymentMethod, request.AmountTendered));
        if (result.IsFailure)
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

        return Ok(result.Value);
    }

    [HasPermission(Permissions.PosVoidBill)]
    [HttpPost("orders/{orderId:guid}/void-or-return")]
    public async Task<IActionResult> VoidOrReturnOrder(Guid orderId, [FromBody] VoidOrReturnRequest request)
    {
        var result = await _sender.Send(new VoidOrReturnOrderCommand(orderId, request.IsReturnRefund, request.Reason, request.ManagerPin));
        if (result.IsFailure)
            return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

        return Ok(new { message = "Đã xử lý Hủy/Hoàn trả đơn hàng, hoàn kho và ghi vết Audit thành công!" });
    }
}

public record RecallCheckoutRequest(ConvenienceStore.Domain.Enums.PaymentMethod PaymentMethod, decimal AmountTendered);
public record VoidOrReturnRequest(bool IsReturnRefund, string Reason, string ManagerPin);
