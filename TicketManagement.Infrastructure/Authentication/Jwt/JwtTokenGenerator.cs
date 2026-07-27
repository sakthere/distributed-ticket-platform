using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TicketManagement.Application.Interfaces;

namespace TicketManagement.Infrastructure.Authentication.Jwt
{
    public class JwtTokenGenerator:IJwtTokenGenerator
    {
        public readonly JwtSettings _settings;
        public JwtTokenGenerator(IOptions<JwtSettings> settings)
        {
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(int userId, string email, string role)
        {
            var claim = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claim,
                expires: expiresAt,
                signingCredentials: credentials);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return (tokenString, expiresAt);
        }

        
    }
}
