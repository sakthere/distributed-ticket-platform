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
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginCommandHandler(IAuthSessionIssuer authSessionIssuer, IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _authSessionIssuer = authSessionIssuer;
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<LoginResult>> HandleAsync(LoginCommand command)
        {
            var user = await _userRepository.GetByEmailAsync(command.Email);
            if(user == null)
            {
                return Result<LoginResult>.Failure(AuthErrors.InvalidCredentails);
            }
            var isPasswordValid = _passwordHasher.Verify(command.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Result<LoginResult>.Failure(AuthErrors.InvalidCredentails);
            }
            var session = await _authSessionIssuer.IssueAsync(user);
            await _refreshTokenRepository.SaveChangesAsync();

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
