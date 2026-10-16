using ConvenienceStore.Api.Authorization;
using ConvenienceStore.Application.Features.Stores.Commands.CreateStore;
using ConvenienceStore.Application.Features.Stores.Commands.UpdateStore;
using ConvenienceStore.Application.Features.Stores.Queries.GetStoreById;
using ConvenienceStore.Application.Features.Stores.Queries.GetStores;
using ConvenienceStore.Domain.Constants;
using ConvenienceStore.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers
{
    [ApiController]
    [Route("api/v1/stores")]
    public class StoresController : ControllerBase
    {
        private readonly ISender _sender;

        public StoresController(ISender sender)
        {
            _sender = sender;
        }

        [HasPermission(Permissions.StoreView)]
        [HttpGet]
        public async Task<IActionResult> GetStores(
            [FromQuery] string? search,
            [FromQuery] StoreStatus? status,
            [FromQuery] string? region,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var result = await _sender.Send(new GetStoresQuery(search, status, region, page, pageSize));
            return Ok(result.Value);
        }

        [HasPermission(Permissions.StoreView)]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetStoreById(Guid id)
        {
            var result = await _sender.Send(new GetStoreByIdQuery(id));
            if (result.IsFailure)
            {
                return NotFound(new { code = result.Error.Code, message = result.Error.Description });
            }

            return Ok(result.Value);
        }

        [HasPermission(Permissions.StoreManage)]
        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] CreateStoreCommand command)
        {
            var result = await _sender.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(new { code = result.Error.Code, message = result.Error.Description });
            }

            return CreatedAtAction(nameof(GetStoreById), new { id = result.Value }, new { id = result.Value });
        }

        [HasPermission(Permissions.StoreManage)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateStore(Guid id, [FromBody] UpdateStoreRequest request)
        {
            var command = new UpdateStoreCommand(
                id,
                request.Name,
                request.Address,
                request.PhoneNumber,
                request.Email,
                request.Is247,
                request.OpeningTime,
                request.ClosingTime,
                request.Latitude,
                request.Longitude,
                request.Region
            );

            var result = await _sender.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(new { code = result.Error.Code, message = result.Error.Description });
            }

            return NoContent();
        }
    }

    public record UpdateStoreRequest(
        string Name,
        string Address,
        string PhoneNumber,
        string? Email,
        bool Is247,
        TimeSpan? OpeningTime,
        TimeSpan? ClosingTime,
        double? Latitude,
        double? Longitude,
        string Region
    );
}
