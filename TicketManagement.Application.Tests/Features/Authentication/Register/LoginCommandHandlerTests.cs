using Microsoft.Extensions.Logging;
using Moq;
using TicketManagement.Application.Features.Authentication;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Features.Authentication.Login;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Tests.Features.Authentication.Register
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IPasswordHasher> _passwordHasher = new();
        private readonly Mock<IAuthSessionIssuer> _authSessionIssuer = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<LoginCommandHandler>> _logger = new();
        private readonly LoginCommandHandler _handler;
        public LoginCommandHandlerTests()
        {
            _handler = new LoginCommandHandler(_authSessionIssuer.Object, _unitOfWork.Object, _userRepository.Object, _passwordHasher.Object, _logger.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidCredentials_ReturnsSuccessWithSession()
        {
            var command = new LoginCommand { Email = "user@example.com", Password = "Password" };

            var existingUser = new User { Id = 1, Email = command.Email, PasswordHash = "STORED-PASSWORD-HASH" };

            _userRepository.Setup(r => r.GetByEmailAsync(command.Email)).ReturnsAsync(existingUser);

            _passwordHasher.Setup(r => r.Verify(command.Password, existingUser.PasswordHash)).Returns(true);

            var fakeSession = new AuthSession
            {
                AccessToken = "fake-access-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = "fake-refresh-token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _authSessionIssuer.Setup(s => s.IssueAsync(existingUser, null)).ReturnsAsync(fakeSession);
            
            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(fakeSession.AccessToken, result.Value.AccessToken);
            Assert.Equal(fakeSession.RefreshToken, result.Value.RefreshToken);
        }

        [Fact]
        public async Task HandleAsync_WithUnknownEmail_ReturnsInvalidCredentialsAndNeverIssuesSession()
        {
            var command = new LoginCommand { Email = "user@example.com", Password = "Password" };
            _userRepository.Setup(s => s.GetByEmailAsync(command.Email)).ReturnsAsync((User?)null);
            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrors.InvalidCredentails, result.Error);

            _authSessionIssuer.Verify(s => s.IssueAsync(It.IsAny<User>(), It.IsAny<Guid>()), Times.Never());
            VerifyLoggedWarningOnce();
        }

        [Fact]
        public async Task HandleAsync_WithWrongPassword_ReturnsInvalidCredentialsAndNeverIssuesSession()
        {
            var command = new LoginCommand { Email = "user@example.com", Password = "Password" };
            var existingUser = new User { Id = 1, Email = "user@example.com", PasswordHash = "SOME-PASSWORD-HASH" };

            _userRepository.Setup(s => s.GetByEmailAsync(command.Email)).ReturnsAsync(existingUser);
            _passwordHasher.Setup(s => s.Verify(command.Password, existingUser.PasswordHash)).Returns(false);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrors.InvalidCredentails, result.Error);

            _authSessionIssuer.Verify(s => s.IssueAsync(It.IsAny<User>(), It.IsAny<Guid>()), Times.Never());
            VerifyLoggedWarningOnce();
        }

        // ILogger<T>.LogWarning(...) is an extension method, not a real member
        // of the ILogger interface - Moq can only intercept actual interface
        // members, so `_logger.Verify(l => l.LogWarning(...))` won't compile.
        // The real member every LogX extension eventually calls is
        // Log<TState>(LogLevel, EventId, TState, Exception?, Func<...>), so
        // that's what has to be verified instead. This exact shape shows up
        // any time a test needs to assert "something got logged" rather than
        // just "the right Result came back."
        private void VerifyLoggedWarningOnce()
        {
            _logger.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((formatter, t) => true)),
                Times.Once);
        }

    } }
