using System.Net.Sockets;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Policies;

namespace TicketManagement.Domain.Entities
{
    public class Ticket:BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public TicketPriority Priority { get; private set; }
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public TicketImpact Impact { get; set; }
        public TicketUrgency Urgency { get; set; }
        public User CreatedByUser { get; set; }
        public User? AssignedToUser { get; set; }
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

        public void RecalculatePriority()
        {
            Priority = TicketPriorityPolicy.Calculate(Impact, Urgency);
        }

        public void UpdateDetails(string title, string description)
        {
            if(Status != TicketStatus.Open)
            {
                throw new InvalidOperationException("Ticket details can only be updated while the ticket is Open");
            }
            Title = title;
            Description = description;

        }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
