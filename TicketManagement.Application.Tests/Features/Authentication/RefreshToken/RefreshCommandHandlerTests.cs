using Moq;
using System;
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Features.Authentication.RefreshToken;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using Xunit;
using RefreshTokenEntity = TicketManagement.Domain.Entities.RefreshToken;

namespace TicketManagement.Application.Tests.Features.Authentication.RefreshToken
{
    public class RefreshCommandHandlerTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
        private readonly Mock<IRefreshTokenHasher> _refreshTokenHasher = new();
        private readonly Mock<IAuthSessionIssuer> _authSessionIssuer = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly RefreshCommandHandler _handler;

        public RefreshCommandHandlerTests()
        {
            _handler = new RefreshCommandHandler(
                _refreshTokenRepository.Object,
                _refreshTokenHasher.Object,
                _authSessionIssuer.Object,
                _userRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidActiveToken_RotatesAndReturnsSuccess()
        {
            var command = new RefreshCommand { RefreshToken = "raw-old-token" };
            var user = new User { Id = 1, Email = "user@example.com" };
            var sessionId = Guid.NewGuid();

            var existingToken = new RefreshTokenEntity
            {
                UserId = user.Id,
                TokenHash = "hashed-old-token",
                SessionId = sessionId,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsRevoked = false
            };

            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("hashed-old-token");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("hashed-old-token")).ReturnsAsync(existingToken);
            _userRepository.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var fakeSession = new AuthSession
            {
                AccessToken = "new-access-token",
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = "new-refresh-token",
                RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            _authSessionIssuer.Setup(s => s.IssueAsync(user, sessionId)).ReturnsAsync(fakeSession);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(fakeSession.AccessToken, result.Value.AccessToken);
            Assert.True(existingToken.IsRevoked); // the OLD token must be dead after rotation
        }

        [Fact]
        public async Task HandleAsync_WithUnknownTokenHash_ReturnsInvalidRefreshToken()
        {
            var command = new RefreshCommand { RefreshToken = "raw-token" };
            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("some-hash");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("some-hash")).ReturnsAsync((RefreshTokenEntity?)null);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrors.InvalidRefreshToken, result.Error);
        }

        [Fact]
        public async Task HandleAsync_WithAlreadyRevokedToken_RevokesFamilyAndReturnsReuseError()
        {
            var command = new RefreshCommand { RefreshToken = "raw-stolen-token" };
            var sessionId = Guid.NewGuid();

            var revokedToken = new RefreshTokenEntity
            {
                UserId = 1,
                TokenHash = "hashed-stolen-token",
                SessionId = sessionId,
                ExpiresAt = DateTime.UtcNow.AddDays(3), // not expired - reuse is caught before expiry even matters
                IsRevoked = true
            };

            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("hashed-stolen-token");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("hashed-stolen-token")).ReturnsAsync(revokedToken);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrors.RefreshTokenReused, result.Error);

            _refreshTokenRepository.Verify(r => r.RevokeSessionFamilyAsync(sessionId), Times.Once);
            _authSessionIssuer.Verify(s => s.IssueAsync(It.IsAny<User>(), It.IsAny<Guid?>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithExpiredToken_ReturnsExpiredError()
        {
            var command = new RefreshCommand { RefreshToken = "raw-old-token" };

            var expiredToken = new RefreshTokenEntity
            {
                UserId = 1,
                TokenHash = "hashed-old-token",
                SessionId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // in the past
                IsRevoked = false
            };

            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("hashed-old-token");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("hashed-old-token")).ReturnsAsync(expiredToken);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(AuthErrors.RefreshTokenExpired, result.Error);
        }
    }
}