using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.Extensions;
using TicketManagement.Application.Features.Authentication.Login;
using TicketManagement.Application.Features.Authentication.Register;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        RegisterCommandHandler _registerCommandHandler;
        LoginCommandHandler _loginCommandHandler;
        public AuthController(RegisterCommandHandler registerCommandHandler, LoginCommandHandler loginCommandHandler)
        {
            _registerCommandHandler = registerCommandHandler;
            _loginCommandHandler = loginCommandHandler;
            
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _registerCommandHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return result.Error.ToActionResult();
            }
            return Created($"api/auth/{result.Value!.UserId}", result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _loginCommandHandler.HandleAsync(command);
            if (result.IsFailure) return result.Error.ToActionResult();
            return Ok(result.Value);
        }
    }
}
