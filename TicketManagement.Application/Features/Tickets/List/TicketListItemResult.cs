using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.List
{
    // Deliberately lighter than TicketResponse/GetTicketByIdResult - a list
    // view doesn't need Description, and shipping it for every row in a page
    // of results is wasted payload for something the UI wouldn't show anyway.
    public class TicketListItemResult
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
