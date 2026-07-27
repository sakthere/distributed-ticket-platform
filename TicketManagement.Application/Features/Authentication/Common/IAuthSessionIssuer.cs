using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Features.Authentication.Common
{
    public interface IAuthSessionIssuer
    {
        Task<AuthSession> IssueAsync(User user, Guid? sessionId = null);
    }
}
