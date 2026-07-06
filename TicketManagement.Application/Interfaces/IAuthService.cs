using System;
using System.Collections.Generic;
using System.Text;
using TicketManagement.Application.DTO.Auth;

namespace TicketManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);

    }
}
