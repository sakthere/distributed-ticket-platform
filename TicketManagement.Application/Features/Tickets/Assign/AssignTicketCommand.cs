namespace TicketManagement.Application.Features.Tickets.Assign
{
    public class AssignTicketCommand
    {
        public int Id { get; set; }
        public int AssigneeUserId { get; set; }
    }
}
