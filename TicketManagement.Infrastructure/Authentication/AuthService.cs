using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement.Application.DTO.Auth;
using TicketManagement.Application.Interfaces;

namespace TicketManagement.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        public readonly IPasswordHasher _passwordHasher;
        public readonly IUserRepository _userRepository;
        public AuthService(IPasswordHasher passwordHasher, IUserRepository userRepository)
        {
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var user = new Domain.Entities.User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = Domain.Enums.UserRole.Employee
            };
            await _userRepository.AddAsync(user);
            
            await _userRepository.SaveChangesAsync();
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request) {
            throw new NotImplementedException();
        }
    }
}
