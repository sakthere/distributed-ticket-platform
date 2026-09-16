using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.Contract.Tickets;
using TicketManagement.Api.Extensions;
using TicketManagement.Application.Features.Tickets.Assign;
using TicketManagement.Application.Features.Tickets.Create;
using TicketManagement.Application.Features.Tickets.Delete;
using TicketManagement.Application.Features.Tickets.Update;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly CreateTicketCommandHandler _createTicketCommandHandler;
        private readonly UpdateTicketCommandHandler _updateTicketCommandHandler;
        private readonly DeleteTicketCommandHandler _deleteTicketCommandHandler;
        private readonly AssignTicketCommandHandler _assignTicketCommandHandler;
        public TicketController(
            CreateTicketCommandHandler createTicketCommandHandler,
            UpdateTicketCommandHandler updateTicketCommandHandler,
            DeleteTicketCommandHandler deleteTicketCommandHandler,
            AssignTicketCommandHandler assignTicketCommandHandler)
        {
            _createTicketCommandHandler = createTicketCommandHandler;
            _updateTicketCommandHandler = updateTicketCommandHandler;
            _deleteTicketCommandHandler = deleteTicketCommandHandler;
            _assignTicketCommandHandler = assignTicketCommandHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTicketCommand command)
        {
            command.CreatedByUserId = User.GetUserId();

            var result = await _createTicketCommandHandler.HandleAsync(command);

            if (result.IsFailure)
            {
                return result.Error.ToActionResult();
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTicketCommand command)
        {
            command.Id = id;
            command.CurrentUserId = User.GetUserId();

            var result = await _updateTicketCommandHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return result.Error.ToActionResult();
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

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteTicketCommand
            {
                Id = id,
                CurrentUserId = User.GetUserId(),
                CurrentUserRole = User.GetUserRole()
            };

            var result = await _deleteTicketCommandHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return result.Error.ToActionResult();
            }

            return NoContent();
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = $"{nameof(UserRole.Agent)},{nameof(UserRole.Admin)}")]
        public async Task<IActionResult> Assign(int id, AssignTicketCommand command)
        {
            command.Id = id;

            var result = await _assignTicketCommandHandler.HandleAsync(command);
            if (result.IsFailure)
            {
                return result.Error.ToActionResult();
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
                AssignedToUserId = result.Value.AssignedToUserId,
                CreatedAt = result.Value.CreatedAt
            };

            return Ok(response);
        }

    }
}
