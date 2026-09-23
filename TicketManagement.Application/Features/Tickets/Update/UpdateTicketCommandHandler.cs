using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Update
{
    public class UpdateTicketCommandHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result<UpdateTicketResult>> HandleAsync(UpdateTicketCommand command)
        {
            var ticket = await _ticketRepository.GetByIdAsync(command.Id);

            if(ticket == null)
            {
                return Result<UpdateTicketResult>.Failure(TicketErrors.NotFound);
            }
            if(ticket.CreatedByUserId != command.CurrentUserId)
            {
                return Result<UpdateTicketResult>.Failure(TicketErrors.NotTicketOwner);
            }
            if (ticket.Status != TicketStatus.Open)
            {
                return Result<UpdateTicketResult>.Failure(TicketErrors.TicketNotEditable);
            }

            ticket.UpdateDetails(command.Title, command.Description);
            await _unitOfWork.SaveChangesAsync();

            return Result<UpdateTicketResult>.Success(new UpdateTicketResult
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Impact = ticket.Impact,
                Urgency = ticket.Urgency,
                CreatedAt = ticket.CreatedAt
            });
        }
    }
}
