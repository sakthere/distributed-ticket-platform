using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Assign
{
    public class AssignTicketCommandHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public AssignTicketCommandHandler(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
        }

        public async Task<Result<AssignTicketResult>> HandleAsync(AssignTicketCommand command)
        {
            var ticket = await _ticketRepository.GetByIdAsync(command.Id);
            if (ticket == null) return Result<AssignTicketResult>.Failure(TicketErrors.NotFound);

            // Checked before touching the user table - a ticket that's already in a
            // terminal status can never be assigned, no matter who the assignee is,
            // so there's no reason to pay for the extra lookup.
            if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Rejected)
            {
                return Result<AssignTicketResult>.Failure(TicketErrors.TicketNotAssignable);
            }

            var assignee = await _userRepository.GetByIdAsync(command.AssigneeUserId);
            if (assignee == null || assignee.Role != UserRole.Agent)
            {
                return Result<AssignTicketResult>.Failure(TicketErrors.InvalidAssignee);
            }

            ticket.AssignTo(assignee.Id);
            await _ticketRepository.SaveChangesAsync();

            return Result<AssignTicketResult>.Success(new AssignTicketResult
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Impact = ticket.Impact,
                Urgency = ticket.Urgency,
                AssignedToUserId = ticket.AssignedToUserId,
                CreatedAt = ticket.CreatedAt
            });
        }
    }
}
