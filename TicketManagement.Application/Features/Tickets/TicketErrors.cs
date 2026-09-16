using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Application.Common;

namespace TicketManagement.Application.Features.Tickets
{
    public static class TicketErrors
    {
        public static readonly Error NotFound = new("TICKET001", "Ticket not found");
        public static readonly Error NotTicketOwner = new("TICKET002", "You do not have permission to edit this ticket");
        public static readonly Error TicketNotEditable = new("TICKET001", "This ticket can no longer be editable");

    }
}
