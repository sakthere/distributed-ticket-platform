using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Interfaces
{
    public interface IRefreshTokenGenerator
    {
        (string Token, DateTime ExpiresAt) Generate();
    }
}
