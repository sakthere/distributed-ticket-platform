using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketManagement.Api.Contract.Tickets;
using TicketManagement.Api.Extensions;
using TicketManagement.Application.Features.Tickets.Create;
using TicketManagement.Application.Features.Tickets.Get;
using TicketManagement.Application.Features.Tickets.Update;

namespace TicketManagement.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly CreateTicketCommandHandler _createTicketCommandHandler;
        private readonly UpdateTicketCommandHandler _updateTicketCommandHandler;
        private readonly GetTicketByIdQueryHandler _getTicketByIdQueryHandler;
        public TicketController(
            CreateTicketCommandHandler createTicketCommandHandler,
            UpdateTicketCommandHandler updateTicketCommandHandler,
            GetTicketByIdQueryHandler getTicketByIdQueryHandler)
        {
            _createTicketCommandHandler = createTicketCommandHandler;
            _updateTicketCommandHandler = updateTicketCommandHandler;
            _getTicketByIdQueryHandler = getTicketByIdQueryHandler;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var query = new GetTicketByIdQuery
            {
                Id = id,
                CurrentUserId = User.GetUserId(),
                CurrentUserRole = User.GetUserRole()
            };

            var result = await _getTicketByIdQueryHandler.HandleAsync(query, cancellationToken);
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
