using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Application.Common;
using TicketManagement.Application.Features.Authentication.Common;
using TicketManagement.Application.Interfaces;

namespace TicketManagement.Application.Features.Authentication.RefreshToken
{
    public class RefreshCommandHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IAuthSessionIssuer _authSessionIssuer;
        private readonly IUserRepository _userRepository;

        public RefreshCommandHandler(IRefreshTokenRepository refreshTokenRepository, IRefreshTokenHasher refreshTokenHasher, IAuthSessionIssuer authSessionIssuer, IUserRepository userRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenHasher = refreshTokenHasher;
            _authSessionIssuer = authSessionIssuer;
            _userRepository = userRepository;
        }
        public async Task<Result<RefreshResult>> HandleAsync(RefreshCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.RefreshToken))
            {
                return Result<RefreshResult>.Failure(AuthErrors.InvalidRefreshToken);
            }
            var incomingHash = _refreshTokenHasher.Hash(command.RefreshToken);
            var existingToken = await _refreshTokenRepository.GetByHashAsync(incomingHash);

            if (existingToken == null)
            {
                return Result<RefreshResult>.Failure(AuthErrors.InvalidRefreshToken);
            }

            if (existingToken.IsRevoked)
            {
                await _refreshTokenRepository.RevokeSessionFamilyAsync(existingToken.SessionId);
                return Result<RefreshResult>.Failure(AuthErrors.RefreshTokenReused);
            }
            if (existingToken.IsExpired)
            {
                return Result<RefreshResult>.Failure(AuthErrors.RefreshTokenExpired);
            }

            var user = await _userRepository.GetByIdAsync(existingToken.UserId);
            if (user == null)
            {
                return Result<RefreshResult>.Failure(AuthErrors.UserNotFound);
            }

            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTime.UtcNow;

            var session = await _authSessionIssuer.IssueAsync(user, existingToken.SessionId);
            await _refreshTokenRepository.SaveChangesAsync();

            return Result<RefreshResult>.Success(
                new RefreshResult
                {
                    AccessToken = session.AccessToken,
                    AccessTokenExpiresAt = session.AccessTokenExpiresAt,
                    RefreshToken = session.RefreshToken,
                    RefreshTokenExpiresAt = session.RefreshTokenExpiresAt
                });
        }
    }
}
