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
        public static readonly Error TicketNotEditable = new("TICKET003", "This ticket can no longer be editable");
        public static readonly Error TicketNotDeletable = new("TICKET004", "This ticket can no longer be deleted");
        public static readonly Error InvalidAssignee = new("TICKET005", "The specified user cannot be assigned tickets");
        public static readonly Error TicketNotAssignable = new("TICKET006", "This ticket cannot be assigned in its current state");
        public static readonly Error InvalidStatusTransition = new("TICKET007", "This status transition is not allowed");
        public static readonly Error NotAssignedAgent = new("TICKET008", "You do not have permission to change this ticket's status");
        public static readonly Error TicketPriorityNotEditable = new("TICKET009", "This ticket's priority can no longer be changed");
        public static readonly Error InvalidPagination = new("TICKET010", "Page must be at least 1 and PageSize must be between 1 and 100");

    }
}
