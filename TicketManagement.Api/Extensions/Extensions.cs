using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.Mappings;
using TicketManagement.Application.Common;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Api.Extensions
{
    public static class Extensions
    {
        public static IActionResult ToActionResult(this Error error)
        {
            return ErrorMapping.ToActionResult(error);
        }
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        public static UserRole GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.Parse<UserRole>(roleClaim!);
        }
    }
}
