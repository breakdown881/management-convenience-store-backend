using ConvenienceStore.Application.Features.System.Queries.Ping;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ConvenienceStore.Api.Controllers
{
    [ApiController]
    [Route("api/v1/system")]
    public class SystemController : ControllerBase
    {
        private readonly ISender _sender;

        public SystemController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping([FromBody] string message)
        {
            var result = await _sender.Send(new PingQuery(message));
            if (result.IsFailure)
            {
                return BadRequest(result.Error);
            }

            return Ok(new { data = result.Value });
        }
    }
}
