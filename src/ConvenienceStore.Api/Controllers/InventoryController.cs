using ConvenienceStore.Api.Authorization;
using ConvenienceStore.Application.Features.Inventory;
using ConvenienceStore.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers
{
    [ApiController]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly ISender _sender;

        public InventoryController(ISender sender)
        {
            _sender = sender;
        }

        [HasPermission(Permissions.InventoryView)]
        [HttpGet("stores/{storeId:guid}")]
        public async Task<IActionResult> GetStoreInventory(Guid storeId)
        {
            var result = await _sender.Send(new GetStoreInventoryQuery(storeId));
            return Ok(result.Value);
        }

        [HasPermission(Permissions.InventoryView)]
        [HttpGet("stores/{storeId:guid}/products/{productId:guid}/stock-card")]
        public async Task<IActionResult> GetStockCard(Guid storeId, Guid productId)
        {
            var result = await _sender.Send(new GetStockCardQuery(storeId, productId));
            return Ok(result.Value);
        }

        [HasPermission(Permissions.InventoryAdjust)]
        [HttpPost("inbound")]
        public async Task<IActionResult> Inbound([FromBody] InboundInventoryCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(new { balanceAfter = result.Value });
        }

        [HasPermission(Permissions.InventoryAdjust)]
        [HttpPost("stocktake")]
        public async Task<IActionResult> ExecuteStocktake([FromBody] ExecuteStocktakeCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(result.Value);
        }

        [HasPermission(Permissions.InventoryScrap)]
        [HttpPost("write-offs")]
        public async Task<IActionResult> CreateWriteOff([FromBody] CreateWriteOffCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(new { id = result.Value });
        }

        [HasPermission(Permissions.InventoryScrap)]
        [HttpPost("write-offs/{id:guid}/approve")]
        public async Task<IActionResult> ApproveWriteOff(Guid id)
        {
            var result = await _sender.Send(new ApproveWriteOffCommand(id));
            if (result.IsFailure)
                return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

            return Ok(new { message = "Đã phê duyệt xuất hủy và trừ tồn kho thành công!" });
        }
    }
}
