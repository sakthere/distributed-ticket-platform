using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
