using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Interfaces
{
    // What the Application layer is allowed to ask ITicketRepository for -
    // deliberately a closed set of primitives/enums, not an IQueryable or a
    // raw SQL fragment, so the repository interface never leaks EF Core (or
    // any persistence detail) up into the Application layer.
    public class TicketListFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public int? CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public TicketListSortBy SortBy { get; set; } = TicketListSortBy.CreatedAt;
        public bool SortDescending { get; set; } = true;
    }

    // A closed enum, not a free-text "sortBy" string, so an invalid or
    // malicious sort field can't reach the ORDER BY clause at all - the only
    // way to add a new sortable field is to add a case here and in the
    // repository's switch, which is exactly the friction that's wanted.
    public enum TicketListSortBy
    {
        CreatedAt,
        Priority,
        Status
    }
}
