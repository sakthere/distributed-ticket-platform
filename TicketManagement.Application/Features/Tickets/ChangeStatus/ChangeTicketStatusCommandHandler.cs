using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using TicketManagement.Domain.Policies;

namespace TicketManagement.Application.Features.Tickets.ChangeStatus
{
    public class ChangeTicketStatusCommandHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ChangeTicketStatusCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ChangeTicketStatusResult>> HandleAsync(ChangeTicketStatusCommand command)
        {
            var ticket = await _ticketRepository.GetByIdAsync(command.Id);
            if (ticket == null) return Result<ChangeTicketStatusResult>.Failure(TicketErrors.NotFound);

            // [Authorize(Roles = Agent,Admin)] on the endpoint already guarantees the
            // caller holds one of those two roles - this check is the resource-based
            // half: Admins can always act, and so can any Agent on a ticket nobody has
            // claimed yet (that's what triaging an unassigned Open ticket means). Once
            // a specific Agent is assigned, only that Agent (or an Admin) may move it
            // further - a different Agent shouldn't be able to resolve or reject work
            // they aren't doing.
            var isAdmin = command.CurrentUserRole == UserRole.Admin;
            var isAssignedAgent = ticket.AssignedToUserId == command.CurrentUserId;
            var isUnassigned = ticket.AssignedToUserId == null;

            if (!isAdmin && !isAssignedAgent && !isUnassigned)
            {
                return Result<ChangeTicketStatusResult>.Failure(TicketErrors.NotAssignedAgent);
            }

            if (!TicketStatusPolicy.IsValidTransition(ticket.Status, command.NewStatus))
            {
                return Result<ChangeTicketStatusResult>.Failure(TicketErrors.InvalidStatusTransition);
            }

            ticket.ChangeStatus(command.NewStatus);
            await _unitOfWork.SaveChangesAsync();

            return Result<ChangeTicketStatusResult>.Success(new ChangeTicketStatusResult
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
