using Microsoft.AspNetCore.Mvc;
using TicketManagement.Application.Common;

namespace TicketManagement.Api.Extensions
{
    public static class Extensions
    {
        public static ActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Value);
            }
            else
            {
                return new BadRequestObjectResult(result.Error);
            }
        }
    }
}
