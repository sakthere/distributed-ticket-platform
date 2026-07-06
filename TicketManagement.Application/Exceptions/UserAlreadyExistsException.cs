using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Application.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string email) : base($"A user with the `{email}` already exists.")
        {}
    }
}
