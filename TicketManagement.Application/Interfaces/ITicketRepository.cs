using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Interfaces
{
    public interface ITicketRepository
    {
        Task AddAsync(Ticket ticket);
        Task SaveChangesAsync();
    }
}
