using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(int userId, string email, string role);
    }
}
