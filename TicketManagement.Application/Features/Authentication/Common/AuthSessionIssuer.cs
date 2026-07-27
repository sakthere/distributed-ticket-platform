using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using RefreshTokenEntity = TicketManagement.Domain.Entities.RefreshToken;

namespace TicketManagement.Application.Features.Authentication.Common
{
    public class AuthSessionIssuer : IAuthSessionIssuer
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public AuthSessionIssuer(
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenRepository refreshTokenRepository,
            IRefreshTokenHasher refreshTokenHasher)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenGenerator = refreshTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenHasher = refreshTokenHasher;
        }

        public async Task<AuthSession> IssueAsync(User user, Guid? sessionId = null)
        {
            var (accessToken, accessTokenExpiresAt) = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, user.Role.ToString());
            var (rawRefreshToken, refreshTokenExpiresAt) = _refreshTokenGenerator.Generate();
            var refreshTokenHash = _refreshTokenHasher.Hash(rawRefreshToken);

            var refreshToken = new RefreshTokenEntity
            {
                UserId = user.Id,
                TokenHash = refreshTokenHash,
                SessionId = sessionId ?? Guid.NewGuid(),
                ExpiresAt = refreshTokenExpiresAt,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new AuthSession
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = rawRefreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }
    }
}