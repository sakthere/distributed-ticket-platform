using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Delete
{
    public class DeleteTicketCommand
    {
        public int Id { get; set; }
        public int CurrentUserId { get; set; }
        public UserRole CurrentUserRole { get; set; }
    }
}
