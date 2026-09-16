using TicketManagement.Domain.Enums;

namespace TicketManagement.Api.Contract.Tickets
{
    public class TicketSummaryResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; set; }
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
