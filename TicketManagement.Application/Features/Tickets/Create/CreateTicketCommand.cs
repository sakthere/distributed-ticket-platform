using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Create
{
    public class CreateTicketCommand
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketImpact Impact { get; set; }
        public TicketUrgency Urgency { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
