using Microsoft.Extensions.Logging;
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Features.Authentication.Login
{
    public class LoginCommandHandler
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthSessionIssuer _authSessionIssuer;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LoginCommandHandler> _logger;

        // ILogger<T> comes from Microsoft.Extensions.Logging.Abstractions, a
        // provider-agnostic package - it does NOT pull Serilog, or any other
        // concrete logging framework, into the Application layer. Serilog is
        // only wired in once, at the API layer's composition root
        // (Program.cs), as the concrete implementation behind this
        // abstraction. Same shape as depending on IUnitOfWork instead of
        // EF Core directly: Application depends on an interface it owns
        // (well, in this case one .NET owns) the meaning of, never on the
        // concrete infrastructure fulfilling it.
        public LoginCommandHandler(IAuthSessionIssuer authSessionIssuer, IUnitOfWork unitOfWork, IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger<LoginCommandHandler> logger)
        {
            _authSessionIssuer = authSessionIssuer;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<LoginResult>> HandleAsync(LoginCommand command)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);
            if(user == null)
            {
                // Logged as a distinct reason internally (useful for spotting a
                // brute-force/credential-stuffing pattern against one email, or
                // account enumeration attempts, in aggregated logs) even though
                // the HTTP response stays the same generic InvalidCredentails
                // for both this case and a wrong password below - the response
                // must not leak which half of the credential pair was wrong.
                // Never log the submitted password, hashed or not.
                _logger.LogWarning("Login failed for {Email}: no account with this email", command.Email);
                return Result<LoginResult>.Failure(AuthErrors.InvalidCredentails);
            }
            var isPasswordValid = _passwordHasher.Verify(command.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed for {Email}: incorrect password", command.Email);
                return Result<LoginResult>.Failure(AuthErrors.InvalidCredentails);
            }
            var session = await _authSessionIssuer.IssueAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<LoginResult>.Success(new LoginResult
            {
                AccessToken = session.AccessToken,
                AccessTokenExpiresAt = session.AccessTokenExpiresAt,
                RefreshToken = session.RefreshToken,
                RefreshTokenExpiresAt = session.RefreshTokenExpiresAt
            });
        }
    }
}
