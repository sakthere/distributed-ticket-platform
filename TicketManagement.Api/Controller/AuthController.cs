using System.Runtime.CompilerServices;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicketManagement.Api.Authentication;
using TicketManagement.Api.Contract;
using TicketManagement.Api.Extensions;
using TicketManagement.Application.Features.Authentication.Login;
using TicketManagement.Application.Features.Authentication.Logout;
using TicketManagement.Application.Features.Authentication.RefreshToken;
using TicketManagement.Application.Features.Authentication.Register;
using static TicketManagement.Api.Contract.AuthResponse;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        RegisterCommandHandler _registerCommandHandler;
        LoginCommandHandler _loginCommandHandler;
        RefreshCommandHandler _refreshCommandHandler;
        LogoutCommandHandler _logoutCommandHandler;
        public AuthController(RegisterCommandHandler registerCommandHandler, LoginCommandHandler loginCommandHandler, RefreshCommandHandler refreshCommandHandler, LogoutCommandHandler logoutCommandHandler)
        {
            _registerCommandHandler = registerCommandHandler;
            _loginCommandHandler = loginCommandHandler;
            _refreshCommandHandler = refreshCommandHandler;
            _logoutCommandHandler = logoutCommandHandler;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand command)
        {
            var result = await _registerCommandHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return result.Error.ToActionResult();
            }
            RefreshTokenCookieWriter.Write(Response, result.Value!.RefreshToken, result.Value.RefreshTokenExpiresAt);
            return Created($"api/auth/{result.Value.UserId}", new RegisterResponse
            {
                UserId = result.Value.UserId,
                AccessToken = result.Value.AccessToken,
                AccessTokenExpiresAt = result.Value.AccessTokenExpiresAt
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _loginCommandHandler.HandleAsync(command);
            if (result.IsFailure) return result.Error.ToActionResult();
            RefreshTokenCookieWriter.Write(Response, result.Value!.RefreshToken, result.Value.RefreshTokenExpiresAt);
            return Ok(new AuthResponse
            {
                AccessToken = result.Value.AccessToken,
                AccessTokenExpiresAt = result.Value.AccessTokenExpiresAt
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var rawToken = Request.Cookies[RefreshTokenCookieWriter.CookieName];
            if (string.IsNullOrEmpty(rawToken))
            {
                return Unauthorized();
            }
            var result = await _refreshCommandHandler.HandleAsync(new RefreshCommand { RefreshToken = rawToken });
            if (result.IsFailure)
            {
                RefreshTokenCookieWriter.Clear(Response);
                return result.Error.ToActionResult();
            }
            RefreshTokenCookieWriter.Write(Response, result.Value!.RefreshToken, result.Value.RefreshTokenExpiresAt);
            return Ok(new AuthResponse
            {
                AccessToken = result.Value.AccessToken,
                AccessTokenExpiresAt = result.Value.AccessTokenExpiresAt
            });


        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rawToken = Request.Cookies[RefreshTokenCookieWriter.CookieName];
            if (!string.IsNullOrEmpty(rawToken))
            {
                await _logoutCommandHandler.HandleAsync(new LogoutCommand { RefreshToken = rawToken });
            }
            RefreshTokenCookieWriter.Clear(Response);
            return NoContent();
        }
    }
}
