using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Domain.Entities
{
    public class TicketComment:BaseEntity
    {
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public Ticket Ticket { get; set; } = null!;
        public User User { get; set; } = null!;
    }
    }
