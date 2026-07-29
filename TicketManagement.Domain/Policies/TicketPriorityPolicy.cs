using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Domain.Policies
{
    public static class TicketPriorityPolicy 
    {
        public static TicketPriority Calculate(TicketImpact impact, TicketUrgency urgency)
        => (impact, urgency) switch
            {
                (TicketImpact.High, TicketUrgency.High) => TicketPriority.Critical,
                (TicketImpact.High, TicketUrgency.Medium) => TicketPriority.High,
                (TicketImpact.Medium, TicketUrgency.High) => TicketPriority.High,
                (TicketImpact.Medium, TicketUrgency.Medium) => TicketPriority.Medium,
                (TicketImpact.High, TicketUrgency.Low) => TicketPriority.Medium,
                (TicketImpact.Low, TicketUrgency.High) => TicketPriority.Medium,
                (TicketImpact.Medium, TicketUrgency.Low) => TicketPriority.Low,
                (TicketImpact.Low, TicketUrgency.Medium) => TicketPriority.Low,
                _ => TicketPriority.Low
            };
        
    }
}
