using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.Contract.Tickets;
using TicketManagement.Api.Extensions;
using TicketManagement.Application.Features.Tickets.Create;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly CreateTicketCommandHandler _createTicketCommandHandler;
        public TicketController(CreateTicketCommandHandler createTicketCommandHandler)
        {
            _createTicketCommandHandler = createTicketCommandHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketCommand command)
        {
            command.CreatedByUserId = User.GetUserId();

            var result = await _createTicketCommandHandler.HandleAsync(command);

            if (result.IsFailure)
            {
                result.Error.ToActionResult();
            }

            var response = new TicketResponse
            {
                Id = result.Value.Id,
                Title = result.Value.Title,
                Description = result.Value.Description,
                Status = result.Value.Status,
                Priority = result.Value.Priority,
                Impact = result.Value.Impact,
                Urgency = result.Value.Urgency,
                CreatedAt = result.Value.CreatedAt
            };
            return Created($"api/tickets/{response.Id}", response);
        }
    }
}
