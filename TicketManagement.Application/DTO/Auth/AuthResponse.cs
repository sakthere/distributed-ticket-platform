using System;
using System.Collections.Generic;
using System.Text;

namespace TicketManagement.Application.DTO.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
