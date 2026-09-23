using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.List
{
    public class GetTicketsQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public TicketListSortBy SortBy { get; set; } = TicketListSortBy.CreatedAt;
        public bool SortDescending { get; set; } = true;
        public int CurrentUserId { get; set; }
        public UserRole CurrentUserRole { get; set; }
    }
}
