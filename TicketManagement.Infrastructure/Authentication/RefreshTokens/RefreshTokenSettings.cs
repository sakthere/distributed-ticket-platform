using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketManagement.Infrastructure.Authentication.RefreshTokens
{
    public class RefreshTokenSettings
    {
        public int ExpiryDays { get; set; } = 7;
    }
}
