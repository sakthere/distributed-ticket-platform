using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Application.Features.Authentication.RefreshToken
{
    public class RefreshCommand
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
