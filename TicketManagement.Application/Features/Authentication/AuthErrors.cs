using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Application.Common;

namespace TicketManagement.Application.Features.Authentication
{
    internal static class AuthErrors
    {
        public static readonly Error InvalidCredentails = new("AUTH001", "Invalid email or password.");
        public static readonly Error EmailAlreadyExists = new ("AUTH002", "An account with this email already exists.");
        public static readonly Error UserNotFound = new("AUTH003", "User not found.");
    }
}
