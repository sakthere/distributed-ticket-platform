using TicketManagement.Application.Common;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Features.Tickets.Create
{
    public class CreateTicketCommandHandler
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CreateTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
        {
            _ticketRepository = ticketRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreateTicketResult>> HandleAsync(CreateTicketCommand command)
        {
            var ticket = new Ticket
            {
                Title = command.Title,
                Description = command.Description,
                Status = TicketStatus.Open,
                Impact = command.Impact,
                Urgency = command.Urgency,
                CreatedByUserId = command.CreatedByUserId
            };
            ticket.RecalculatePriority();

            await _ticketRepository.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            return Result<CreateTicketResult>.Success(new CreateTicketResult
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Impact = ticket.Impact,
                Urgency = ticket.Urgency,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt
            });
        }
    }
}
