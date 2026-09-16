using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;

namespace TicketManagement.Application.Features.Authentication.Logout
{
    public class LogoutCommandHandler
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, IRefreshTokenHasher refreshTokenHasher, IUnitOfWork unitOfWork)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _refreshTokenHasher = refreshTokenHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(LogoutCommand command)
        {
            if (string.IsNullOrEmpty(command.RefreshToken))
            {
                return Result.Success();
            }
            var hash = _refreshTokenHasher.Hash(command.RefreshToken);
            var token = await _refreshTokenRepository.GetByHashAsync(hash);
            if(token == null || token.IsRevoked)
            {
                return Result.Success();
            }
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
            return Result.Success();
        }
    }
}
