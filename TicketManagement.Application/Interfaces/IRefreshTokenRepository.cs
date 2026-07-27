using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByHashAsync(string tokenHash);
        Task RevokeSessionFamilyAsync(Guid sessionId);
        Task SaveChangesAsync();
    }
}
