using ConvenienceStore.Api.Authorization;
using ConvenienceStore.Application.Features.Catalog;
using ConvenienceStore.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers
{
    [ApiController]
    [Route("api/v1/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ISender _sender;

        public CatalogController(ISender sender)
        {
            _sender = sender;
        }

        [HasPermission(Permissions.CatalogManage)]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(new { id = result.Value });
        }

        [HasPermission(Permissions.CatalogManage)]
        [HttpPost("brands")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(new { id = result.Value });
        }

        [HasPermission(Permissions.CatalogManage)]
        [HttpPost("units")]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitOfMeasureCommand command)
        {
            var result = await _sender.Send(command);
            return Ok(new { id = result.Value });
        }

        [HasPermission(Permissions.CatalogManage)]
        [HttpPost("products")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            var result = await _sender.Send(command);
            if (result.IsFailure)
                return BadRequest(new { code = result.Error.Code, message = result.Error.Description });

            return Ok(new { id = result.Value });
        }

        [HasPermission(Permissions.CatalogView)]
        [HttpGet("shelf-tags")]
        public async Task<IActionResult> GetShelfTags([FromQuery] Guid? categoryId)
        {
            var result = await _sender.Send(new GetShelfTagsQuery(categoryId));
            return Ok(result.Value);
        }
    }
}
