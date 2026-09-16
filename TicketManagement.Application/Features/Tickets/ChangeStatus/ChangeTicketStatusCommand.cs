using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.ChangeStatus
{
    public class ChangeTicketStatusCommand
    {
        public int Id { get; set; }
        public TicketStatus NewStatus { get; set; }
        public int CurrentUserId { get; set; }
        public UserRole CurrentUserRole { get; set; }
    }
}
