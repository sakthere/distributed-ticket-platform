using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Persistence.Context;

namespace TicketManagement.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;
        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }
        public async Task RevokeSessionFamilyAsync(Guid sessionId)
        {
            await _context.RefreshTokens.Where(rt => rt.SessionId == sessionId && !rt.IsRevoked).
                ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true).SetProperty(rt => rt.RevokedAt, DateTime.UtcNow));
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
