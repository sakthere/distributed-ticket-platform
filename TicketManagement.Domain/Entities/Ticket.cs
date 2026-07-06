using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Entities
{
    public class Ticket:BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }

        public User CreatedByUser { get; set; }
        public User? AssignedByUser { get; set; }
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}
