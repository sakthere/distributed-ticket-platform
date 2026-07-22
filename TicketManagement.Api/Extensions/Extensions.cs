using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.Mappings;
using TicketManagement.Application.Common;

namespace TicketManagement.Api.Extensions
{
    public static class Extensions
    {
        public static IActionResult ToActionResult(this Error error)
        {
            return ErrorMapping.ToActionResult(error);
        }
    }
}
