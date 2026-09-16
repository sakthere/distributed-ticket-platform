using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.OverridePriority;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace TicketManagement.Application.Tests.Features.Tickets.OverridePriority
{
    public class OverrideTicketPriorityCommandHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly OverrideTicketPriorityCommandHandler _handler;

        public OverrideTicketPriorityCommandHandlerTests()
        {
            _handler = new OverrideTicketPriorityCommandHandler(_ticketRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenAssignedAgentOverridesPriority_RecalculatesAndReturnsSuccess()
        {
            var ticket = new TicketEntity
            {
                Id = 1,
                Status = TicketStatus.InProgress,
                CreatedByUserId = 7,
                AssignedToUserId = 10,
                Impact = TicketImpact.Low,
                Urgency = TicketUrgency.Low
            };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new OverrideTicketPriorityCommand
            {
                Id = 1,
                Impact = TicketImpact.High,
                Urgency = TicketUrgency.High,
                CurrentUserId = 10,
                CurrentUserRole = UserRole.Agent
            };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(TicketImpact.High, ticket.Impact);
            Assert.Equal(TicketUrgency.High, ticket.Urgency);
            Assert.Equal(TicketPriority.Critical, ticket.Priority);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenAdminOverridesPriorityOnTicketAssignedToSomeoneElse_ReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Assigned, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new OverrideTicketPriorityCommand { Id = 1, Impact = TicketImpact.Medium, Urgency = TicketUrgency.Medium, CurrentUserId = 999, CurrentUserRole = UserRole.Admin };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandleAsync_WhenAnyAgentOverridesPriorityOnAnUnassignedTicket_ReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 7, AssignedToUserId = null };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new OverrideTicketPriorityCommand { Id = 1, Impact = TicketImpact.High, Urgency = TicketUrgency.Medium, CurrentUserId = 55, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketDoesNotExist_ReturnsNotFound()
        {
            _ticketRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TicketEntity?)null);

            var command = new OverrideTicketPriorityCommand { Id = 99, Impact = TicketImpact.High, Urgency = TicketUrgency.High, CurrentUserId = 1, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotFound, result.Error);
        }

        [Fact]
        public async Task HandleAsync_WhenADifferentAgentTargetsAnAlreadyAssignedTicket_ReturnsNotAssignedAgent()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Assigned, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new OverrideTicketPriorityCommand { Id = 1, Impact = TicketImpact.High, Urgency = TicketUrgency.High, CurrentUserId = 999, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotAssignedAgent, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Theory]
        [InlineData(TicketStatus.Resolved)]
        [InlineData(TicketStatus.Closed)]
        [InlineData(TicketStatus.Rejected)]
        public async Task HandleAsync_WhenTicketIsInTerminalStatus_ReturnsTicketPriorityNotEditable(TicketStatus status)
        {
            var ticket = new TicketEntity { Id = 1, Status = status, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new OverrideTicketPriorityCommand { Id = 1, Impact = TicketImpact.High, Urgency = TicketUrgency.High, CurrentUserId = 10, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.TicketPriorityNotEditable, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
