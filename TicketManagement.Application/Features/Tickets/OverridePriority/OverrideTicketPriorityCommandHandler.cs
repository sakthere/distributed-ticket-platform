using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.OverridePriority
{
    public class OverrideTicketPriorityCommandHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        public OverrideTicketPriorityCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<OverrideTicketPriorityResult>> HandleAsync(OverrideTicketPriorityCommand command)
        {
            var ticket = await _ticketRepository.GetByIdAsync(command.Id);
            if (ticket == null) return Result<OverrideTicketPriorityResult>.Failure(TicketErrors.NotFound);

            // Same resource-based shape as ChangeTicketStatusCommandHandler, and for the
            // same reason: an Admin can always act, any Agent can act on a ticket nobody
            // has claimed yet (adjusting Impact/Urgency during triage, before assignment),
            // but once a specific Agent owns the ticket, only that Agent or an Admin may
            // re-prioritize it.
            var isAdmin = command.CurrentUserRole == UserRole.Admin;
            var isAssignedAgent = ticket.AssignedToUserId == command.CurrentUserId;
            var isUnassigned = ticket.AssignedToUserId == null;

            if (!isAdmin && !isAssignedAgent && !isUnassigned)
            {
                return Result<OverrideTicketPriorityResult>.Failure(TicketErrors.NotAssignedAgent);
            }

            if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Rejected)
            {
                return Result<OverrideTicketPriorityResult>.Failure(TicketErrors.TicketPriorityNotEditable);
            }

            ticket.OverridePriority(command.Impact, command.Urgency);
            await _unitOfWork.SaveChangesAsync();

            return Result<OverrideTicketPriorityResult>.Success(new OverrideTicketPriorityResult
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
