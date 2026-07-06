using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Domain.Entities
{
    public class RefreshToken:BaseEntity
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
