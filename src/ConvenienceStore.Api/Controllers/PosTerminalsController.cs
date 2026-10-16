using ConvenienceStore.Api.Authorization;
using ConvenienceStore.Application.Features.PosTerminals.Commands.CreatePosTerminal;
using ConvenienceStore.Application.Features.PosTerminals.Commands.RegisterPosDevice;
using ConvenienceStore.Application.Features.PosTerminals.Queries.GetPosTerminalsByStore;
using ConvenienceStore.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class PosTerminalsController : ControllerBase
    {
        private readonly ISender _sender;

        public PosTerminalsController(ISender sender)
        {
            _sender = sender;
        }

        [HasPermission(Permissions.StoreView)]
        [HttpGet("stores/{storeId:guid}/terminals")]
        public async Task<IActionResult> GetTerminalsByStore(Guid storeId)
        {
            var result = await _sender.Send(new GetPosTerminalsByStoreQuery(storeId));
            return Ok(result.Value);
        }

        [HasPermission(Permissions.TerminalManage)]
        [HttpPost("stores/{storeId:guid}/teminals")]
        public async Task<IActionResult> CreateTerminal(Guid storeId, [FromBody] CreateTerminalRequest request)
        {
            var result = await _sender.Send(new CreatePosTerminalCommand(storeId, request.TerminalCode, request.Name));
            if (result.IsFailure)
            {
                return BadRequest(new { code = result.Error.Code, message = result.Error.Description });
            }

            return Ok(new { id = result.Value });
        }

        [HasPermission(Permissions.TerminalManage)]
        [HttpPost("terminals/{terminalId:guid}/register-device")]
        public async Task<IActionResult> RegisterDevice(Guid terminalId, [FromBody] RegisterDeviceRequest request)
        {
            var result = await _sender.Send(new RegisterPosDeviceCommand(terminalId, request.DeviceFingerprint));
            if (result.IsFailure)
            {
                return BadRequest(new { code = result.Error.Code, messgae = result.Error.Description });
            }

            return Ok(new { message = "Đăng ký thiết bị phần cứng POS thành công!" });
        }
    }

    public record CreateTerminalRequest(string TerminalCode, string Name);
    public record RegisterDeviceRequest(string DeviceFingerprint);
}
