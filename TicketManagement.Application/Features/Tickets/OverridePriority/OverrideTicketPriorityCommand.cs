using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.OverridePriority
{
    public class OverrideTicketPriorityCommand
    {
        public int Id { get; set; }
        public TicketImpact Impact { get; set; }
        public TicketUrgency Urgency { get; set; }
        public int CurrentUserId { get; set; }
        public UserRole CurrentUserRole { get; set; }
    }
}
