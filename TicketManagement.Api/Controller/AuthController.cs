using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Features.Authentication.Register;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        RegisterCommandHandler _registerCommandHandler;
        public AuthController(RegisterCommandHandler registerCommandHandler)
        {
            _registerCommandHandler = registerCommandHandler;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _registerCommandHandler.HandleAsync(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Error);
            }

            return Created($"api/auth/{result.Value!.UserId}", result.Value);
        }
    }
}
