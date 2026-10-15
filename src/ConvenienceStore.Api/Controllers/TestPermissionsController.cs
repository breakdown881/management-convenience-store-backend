using ConvenienceStore.Api.Authorization;
using ConvenienceStore.Application.Common.Interfaces;
using ConvenienceStore.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers;

[ApiController]
[Route("api/v1/test-permissions")]
public class TestPermissionsController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    // Inject thêm IAuditService vào constructor:
    private readonly IAuditService _auditService;

    public TestPermissionsController(
        ICurrentUserService currentUserService,
        IAuditService auditService
    )
    {
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    /// <summary>
    /// Endpoint 1: Bất kỳ ai đăng nhập hợp lệ đều truy cập được
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUserInfo()
    {
        return Ok(new
        {
            message = "Xác thực thành công!",
            userId = _currentUserService.UserId,
            username = _currentUserService.Username,
            storeId = _currentUserService.StoreId
        });
    }

    /// <summary>
    /// Endpoint 2: Yêu cầu quyền Thanh toán đơn hàng (pos:checkout)
    /// </summary>
    [HasPermission(Permissions.PosCheckout)]
    [HttpPost("pos/checkout")]
    public IActionResult TestPosCheckout()
    {
        return Ok(new { message = "Cho phép thực hiện thanh toán tại quầy POS." });
    }

    /// <summary>
    /// Endpoint 3: Yêu cầu quyền Hủy hóa đơn (pos:void_bill)
    /// </summary>
    [HasPermission(Permissions.PosVoidBill)]
    [HttpPost("pos/void-bill")]
    public async Task<IActionResult> TestPosVoidBill([FromBody] VoidBillRequest request)
    {
        // Giả lập ghi nhận Audit Trail khi có thao tác Hủy hóa đơn
        await _auditService.LogSensitiveActionAsync(
        action: "POS_VOID_BILL",
        entityName: "Order",
        entityId: request.OrderId,
        oldValues: new { Status = "Completed", Amount = 150000 },
        newValues: new { Status = "Voided", Reason = request.Reason });

        return Ok(new { message = $"Đã hủy hóa đơn {request.OrderId} và ghi vết Audit Trail thành công!" });
    }

    public record VoidBillRequest(string OrderId, string Reason);

    /// <summary>
    /// Endpoint 4: Yêu cầu quyền Quản lý Danh mục hàng hóa (catalog:manage)
    /// </summary>
    [HasPermission(Permissions.CatalogManage)]
    [HttpPost("catalog/manage")]
    public IActionResult TestCatalogManage()
    {
        return Ok(new { message = "Cho phép thực hiện cập nhật danh mục hàng hóa." });
    }
}
