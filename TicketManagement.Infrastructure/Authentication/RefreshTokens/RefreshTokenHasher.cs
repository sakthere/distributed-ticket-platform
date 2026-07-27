using System.Security.Cryptography;
using System.Text;
using TicketManagement.Application.Interfaces;

namespace TicketManagement.Infrastructure.Authentication.RefreshTokens
{
    public class RefreshTokenHasher: IRefreshTokenHasher
    {
        public string Hash(string rawToken)
        {
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
