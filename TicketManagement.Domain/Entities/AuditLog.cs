using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Domain.Entities
{
    public class AuditLog:BaseEntity
    {
        public string EntityName { get; set; }
        public string Action { get; set; }
        public string Changes { get; set; } = string.Empty;
        public int PerformedByUserId { get; set; }
    }
}
