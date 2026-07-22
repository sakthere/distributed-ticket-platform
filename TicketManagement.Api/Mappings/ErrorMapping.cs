using TicketManagement.Application.Features.Authentication;
using Microsoft.AspNetCore.Http;
using TicketManagement.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace TicketManagement.Api.Mappings
{
    public class ErrorMapping
    {
        public static IActionResult ToActionResult(Error error)
        {
            var statusCode = error switch
            {
                var e when e == AuthErrors.InvalidCredentails => StatusCodes.Status401Unauthorized,
                var e when e == AuthErrors.EmailAlreadyExists => StatusCodes.Status409Conflict,
                var e when e == AuthErrors.UserNotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };

            var problemDetails = new ProblemDetails
            {
                Title = error.Code,
                Detail = error.Description,
                Status = statusCode
            };

            return new ObjectResult(problemDetails)
            {
                StatusCode = statusCode
            };
        } 
        public static int GetStatusCode(Error error)
        {
            if(error == AuthErrors.InvalidCredentails)
            {
                return StatusCodes.Status401Unauthorized;
            }
            if(error == AuthErrors.EmailAlreadyExists)
            {
                return StatusCodes.Status409Conflict;
            }
            if(error == AuthErrors.UserNotFound)
            {
                return StatusCodes.Status404NotFound;
            }
            return StatusCodes.Status400BadRequest;

        }
    }
}
