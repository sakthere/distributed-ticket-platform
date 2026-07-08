
using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Authentication.Register
{
    public class RegisterCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<RegisterResult>> HandleAsync(RegisterCommand command)
        {
            var existingUser = await _userRepository.GetByEmailAsync(command.Email);
            
            if(existingUser != null)
            {
                return Result<RegisterResult>.Failure("This Email Already Exists");
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

            return Result<RegisterResult>.Success(new RegisterResult
            {
                UserId = user.Id
            });
        }
    }
}
