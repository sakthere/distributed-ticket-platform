using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Features.Authentication.Login
{
    public class LoginCommandHandler
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUserRepository _userRepository;

        public LoginCommandHandler(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
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
            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.ToString());

            return Result<LoginResult>.Success(new LoginResult
            {
                AccessToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            });
        }
    }
}
