using System.Runtime.InteropServices;
using Moq;
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Features.Authentication.Register;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using Xunit;

namespace TicketManagement.Application.Tests.Features.Authentication.Register
{
    public class RegisterCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IPasswordHasher> _passwordHasher= new();
        private readonly Mock<IAuthSessionIssuer> _authSessionIssuer= new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
        private readonly RegisterCommandHandler _handler;

        public RegisterCommandHandlerTests()
        {
            _handler = new RegisterCommandHandler(_userRepository.Object, _passwordHasher.Object, _authSessionIssuer.Object, _refreshTokenRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WithNewEmail_ReturnSuccessWithSession()
        {
            var command = new RegisterCommand
            {
                Email = "new.user@example.com",
                Password = "password"
            };

            _userRepository
                .Setup(r => r.GetByEmailAsync(command.Email))
                .ReturnsAsync((User?)null);

            _passwordHasher.Setup(r => r.Hash(command.Password)).Returns("hashed-password");

            var fakeSession = new AuthSession
            {
                AccessToken = "fake-access-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = "fake-refresh-token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _authSessionIssuer.Setup(s => s.IssueAsync(It.IsAny<User>(), null)).ReturnsAsync(fakeSession);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(fakeSession.AccessToken, result.Value.AccessToken);
            Assert.Equal(fakeSession.RefreshToken, result.Value.RefreshToken);
        }

        [Fact]
        public async Task HandleAsync_WithExistingEmail_ReturnsFailureAndNeverIssuesSession()
        {
            var command = new RegisterCommand
            {
                Email = "already.registered@example.com",
                Password = "Password"
            };
            var existingUser = new User { Id = 1, Email = command.Email };
            
            _userRepository.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existingUser);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrors.EmailAlreadyExists, result.Error);

            _authSessionIssuer.Verify(s => s.IssueAsync(It.IsAny<User>(), It.IsAny<Guid?>()), Times.Never);
        }
    }
}
