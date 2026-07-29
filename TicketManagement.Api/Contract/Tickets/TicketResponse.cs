using TicketManagement.Domain.Enums;

namespace TicketManagement.Api.Contract.Tickets
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public TicketImpact Impact { get; set; }
        public TicketUrgency Urgency { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
