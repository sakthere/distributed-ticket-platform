using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Delete
{
    public class DeleteTicketCommandHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        public DeleteTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> HandleAsync(DeleteTicketCommand command)
        {
            var ticket = await _ticketRepository.GetByIdAsync(command.Id);
            if (ticket == null) return Result.Failure(TicketErrors.NotFound);

            var isAdmin = command.CurrentUserRole == UserRole.Admin;
            var isOwner = ticket.CreatedByUserId == command.CurrentUserId;

            if (!isAdmin && !isOwner) return Result.Failure(TicketErrors.NotTicketOwner);

            // Admins may delete a ticket in any status. Owners may only delete
            // their own ticket while it is still Open - once work has started
            // on it, deleting it out from under an agent would be surprising.
            if (!isAdmin && ticket.Status != TicketStatus.Open) return Result.Failure(TicketErrors.TicketNotDeletable);

            ticket.Delete();
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
    }
}
