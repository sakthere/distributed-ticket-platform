using System.Threading;
using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Get
{
    public class GetTicketByIdQueryHandler
    {
        private readonly ITicketRepository _ticketRepository;
        public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<Result<GetTicketByIdResult>> HandleAsync(GetTicketByIdQuery query, CancellationToken cancellationToken = default)
        {
            var ticket = await _ticketRepository.GetByIdAsync(query.Id, cancellationToken);
            if (ticket == null) return Result<GetTicketByIdResult>.Failure(TicketErrors.NotFound);

            // Admins and Agents can view any ticket (they need visibility to triage and
            // work tickets that aren't theirs). Employees can only view tickets they
            // created themselves - this is the same ownership rule as Update/Delete,
            // just read-only.
            var canView = query.CurrentUserRole is UserRole.Admin or UserRole.Agent
                || ticket.CreatedByUserId == query.CurrentUserId;

            if (!canView) return Result<GetTicketByIdResult>.Failure(TicketErrors.NotTicketOwner);

            return Result<GetTicketByIdResult>.Success(new GetTicketByIdResult
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Impact = ticket.Impact,
                Urgency = ticket.Urgency,
                CreatedByUserId = ticket.CreatedByUserId,
                AssignedToUserId = ticket.AssignedToUserId,
                CreatedAt = ticket.CreatedAt
            });
        }
    }
}
