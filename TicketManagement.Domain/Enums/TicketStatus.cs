using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Domain.Enums;
    public enum TicketStatus
    {
        Open = 1,
        Assigned = 2,
        InProgress = 3,
        Resolved = 4,
        Closed = 5,
        Rejected = 6
    }

