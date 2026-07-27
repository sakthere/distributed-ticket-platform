using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Features.Authentication.Logout
{
    public class LogoutCommand
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}

