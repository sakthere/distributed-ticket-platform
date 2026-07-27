using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TicketManagement.Application.Interfaces;

namespace TicketManagement.Infrastructure.Authentication.RefreshTokens
{
    public class RefreshTokenGenerator:IRefreshTokenGenerator
    {
        private readonly RefreshTokenSettings _settings;
        public RefreshTokenGenerator(IOptions<RefreshTokenSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiresAt) Generate()
        {
            var randomByte = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomByte)
                .Replace("+","-").Replace("/", "_").Replace("=", "");
            var expiresAt = DateTime.UtcNow.AddDays(_settings.ExpiryDays);
            return (token, expiresAt);
        }
    }
}
