
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Authentication.Register
{
    public class RegisterCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthSessionIssuer _authSessionIssuer;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuthSessionIssuer authSessionIssuer, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _authSessionIssuer = authSessionIssuer;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Result<RegisterResult>> HandleAsync(RegisterCommand command)
        {
            var existingUser = await _userRepository.GetByEmailAsync(command.Email);

            if(existingUser != null)
            {
                return Result<RegisterResult>.Failure(AuthErrors.EmailAlreadyExists);
            }

            var user = new User
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = command.Email,
                PasswordHash = _passwordHasher.Hash(command.Password),
                Role =  UserRole.Employee,
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var session = await _authSessionIssuer.IssueAsync(user);
            await _refreshTokenRepository.SaveChangesAsync();

            return Result<RegisterResult>.Success(new RegisterResult
            {
                UserId = user.Id,
                AccessToken = session.AccessToken,
                AccessTokenExpiresAt = session.AccessTokenExpiresAt,
                RefreshToken = session.RefreshToken,
                RefreshTokenExpiresAt = session.RefreshTokenExpiresAt
            });
        }
    }
}
