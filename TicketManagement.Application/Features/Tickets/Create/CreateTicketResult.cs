using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Create
{
    public class CreateTicketResult
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketImpact Impact { get; set; }
        public TicketUrgency Urgency { get; set; }
        public TicketPriority Priority { get; set; }
        public DateTime CreatedAt {  get; set; }
    }
}
