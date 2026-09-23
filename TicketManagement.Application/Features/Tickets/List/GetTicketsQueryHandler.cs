using System.Threading;
using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.List
{
    public class GetTicketsQueryHandler
    {
        private readonly ITicketRepository _ticketRepository;
        public GetTicketsQueryHandler(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<Result<PagedResult<TicketListItemResult>>> HandleAsync(GetTicketsQuery query, CancellationToken cancellationToken = default)
        {
            if (query.Page < 1 || query.PageSize < 1 || query.PageSize > 100)
            {
                return Result<PagedResult<TicketListItemResult>>.Failure(TicketErrors.InvalidPagination);
            }

            var filter = new TicketListFilter
            {
                Page = query.Page,
                PageSize = query.PageSize,
                Status = query.Status,
                Priority = query.Priority,
                CreatedByUserId = query.CreatedByUserId,
                AssignedToUserId = query.AssignedToUserId,
                SortBy = query.SortBy,
                SortDescending = query.SortDescending
            };

            // Employees only ever see their own tickets - this overrides whatever
            // CreatedByUserId the client sent (including none at all), the same way
            // Get Ticket's ownership check ignores who's asking and looks at who
            // actually created the ticket. Agent/Admin get the filter as requested,
            // including no creator filter at all (see everyone's tickets).
            if (query.CurrentUserRole == UserRole.Employee)
            {
                filter.CreatedByUserId = query.CurrentUserId;
            }

            var (items, totalCount) = await _ticketRepository.GetPagedAsync(filter, cancellationToken);

            var mapped = items.Select(t => new TicketListItemResult
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CreatedByUserId = t.CreatedByUserId,
                AssignedToUserId = t.AssignedToUserId,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Result<PagedResult<TicketListItemResult>>.Success(new PagedResult<TicketListItemResult>
            {
                Items = mapped,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }
    }
}
