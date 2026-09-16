using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Persistence.Context;

namespace TicketManagement.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;
        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
        }

        public async Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
           return await _context.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetPagedAsync(TicketListFilter filter, CancellationToken cancellationToken = default)
        {
            // The global IsDeleted query filter on Ticket (see TicketConfiguration)
            // already applies here automatically - no need to repeat it.
            var query = _context.Tickets.AsQueryable();

            if (filter.Status.HasValue) query = query.Where(t => t.Status == filter.Status.Value);
            if (filter.Priority.HasValue) query = query.Where(t => t.Priority == filter.Priority.Value);
            if (filter.CreatedByUserId.HasValue) query = query.Where(t => t.CreatedByUserId == filter.CreatedByUserId.Value);
            if (filter.AssignedToUserId.HasValue) query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);

            query = (filter.SortBy, filter.SortDescending) switch
            {
                (TicketListSortBy.Priority, true) => query.OrderByDescending(t => t.Priority),
                (TicketListSortBy.Priority, false) => query.OrderBy(t => t.Priority),
                (TicketListSortBy.Status, true) => query.OrderByDescending(t => t.Status),
                (TicketListSortBy.Status, false) => query.OrderBy(t => t.Status),
                (TicketListSortBy.CreatedAt, true) => query.OrderByDescending(t => t.CreatedAt),
                _ => query.OrderBy(t => t.CreatedAt)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

    }
}
