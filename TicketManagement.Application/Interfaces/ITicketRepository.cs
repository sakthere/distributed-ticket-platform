using System.Threading;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Interfaces
{
    public interface ITicketRepository
    {
        Task AddAsync(Ticket ticket);
        Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(TicketListFilter filter, CancellationToken cancellationToken = default);
    }
}
