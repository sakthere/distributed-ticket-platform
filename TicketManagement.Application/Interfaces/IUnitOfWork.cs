using System.Threading;

namespace TicketManagement.Application.Interfaces
{
    // EF Core's DbContext is already a Unit of Work in disguise - every
    // repository sharing one scoped DbContext instance commits everything
    // else that instance is tracking whenever any one of them calls
    // SaveChangesAsync, whether that's obvious from the repository's own
    // interface or not. This makes that fact explicit and gives handlers a
    // single, honest place to say "commit now" instead of picking an
    // arbitrary repository to call SaveChangesAsync on.
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
