using Moq;
using System;
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication.Logout;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using Xunit;
using RefreshTokenEntity = TicketManagement.Domain.Entities.RefreshToken;

namespace TicketManagement.Application.Tests.Features.Authentication.Logout
{
    public class LogoutCommandHandlerTests
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
        private readonly Mock<IRefreshTokenHasher> _refreshTokenHasher = new();
        private readonly LogoutCommandHandler _handler;

        public LogoutCommandHandlerTests()
        {
            _handler = new LogoutCommandHandler(_refreshTokenRepository.Object, _refreshTokenHasher.Object);
        }

        [Fact]
        public async Task HandleAsync_WithEmptyToken_ReturnsSuccessAndNeverTouchesRepository()
        {
            var command = new LogoutCommand { RefreshToken = string.Empty };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            _refreshTokenHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
            _refreshTokenRepository.Verify(r => r.GetByHashAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithUnknownToken_ReturnsSuccessWithoutSaving()
        {
            var command = new LogoutCommand { RefreshToken = "raw-token" };

            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("some-hash");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("some-hash")).ReturnsAsync((RefreshTokenEntity?)null);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            _refreshTokenRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithAlreadyRevokedToken_ReturnsSuccessWithoutDoubleRevoking()
        {
            var command = new LogoutCommand { RefreshToken = "raw-token" };

            var alreadyRevokedToken = new RefreshTokenEntity
            {
                UserId = 1,
                TokenHash = "some-hash",
                SessionId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsRevoked = true,
                RevokedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("some-hash");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("some-hash")).ReturnsAsync(alreadyRevokedToken);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            _refreshTokenRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WithValidActiveToken_RevokesTokenAndReturnsSuccess()
        {
            var command = new LogoutCommand { RefreshToken = "raw-token" };

            var activeToken = new RefreshTokenEntity
            {
                UserId = 1,
                TokenHash = "some-hash",
                SessionId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsRevoked = false
            };

            _refreshTokenHasher.Setup(h => h.Hash(command.RefreshToken)).Returns("some-hash");
            _refreshTokenRepository.Setup(r => r.GetByHashAsync("some-hash")).ReturnsAsync(activeToken);

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.True(activeToken.IsRevoked);
            Assert.NotNull(activeToken.RevokedAt);
            _refreshTokenRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
