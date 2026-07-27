using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Domain.Entities
{
    public class RefreshToken:BaseEntity
    {
        public int UserId { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public Guid SessionId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
