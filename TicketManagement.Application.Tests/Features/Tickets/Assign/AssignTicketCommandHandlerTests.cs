using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.Assign;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;
using UserEntity = TicketManagement.Domain.Entities.User;

namespace TicketManagement.Application.Tests.Features.Tickets.Assign
{
    public class AssignTicketCommandHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly AssignTicketCommandHandler _handler;

        public AssignTicketCommandHandlerTests()
        {
            _handler = new AssignTicketCommandHandler(_ticketRepository.Object, _userRepository.Object);
        }

        private static UserEntity Agent(int id) => new()
        {
            Id = id,
            FirstName = "Agent",
            LastName = "Smith",
            Email = $"agent{id}@example.com",
            PasswordHash = "hash",
            Role = UserRole.Agent
        };

        [Fact]
        public async Task HandleAsync_WhenAssigneeIsAValidAgentAndTicketIsOpen_AssignsAndReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 7 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
            _userRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Agent(10));

            var command = new AssignTicketCommand { Id = 1, AssigneeUserId = 10 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(TicketStatus.Assigned, ticket.Status);
            Assert.Equal(10, ticket.AssignedToUserId);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenReassigningANonTerminalTicket_ReassignsToNewAgentWithoutResettingStatus()
        {
            // Reassignment (Assigned/InProgress -> a different agent) must not reset
            // progress that's already been made - only the *first* assignment (Open
            // -> Assigned) should move the status. See Ticket.AssignTo.
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.InProgress, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
            _userRepository.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(Agent(20));

            var command = new AssignTicketCommand { Id = 1, AssigneeUserId = 20 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(20, ticket.AssignedToUserId);
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
        }

        [Fact]
        public async Task HandleAsync_WhenFirstAssigningAnOpenTicket_TransitionsStatusToAssigned()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 7 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
            _userRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(Agent(10));

            var command = new AssignTicketCommand { Id = 1, AssigneeUserId = 10 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(TicketStatus.Assigned, ticket.Status);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketDoesNotExist_ReturnsNotFound()
        {
            _ticketRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TicketEntity?)null);

            var command = new AssignTicketCommand { Id = 99, AssigneeUserId = 10 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotFound, result.Error);
        }

        [Theory]
        [InlineData(TicketStatus.Resolved)]
        [InlineData(TicketStatus.Closed)]
        [InlineData(TicketStatus.Rejected)]
        public async Task HandleAsync_WhenTicketIsInTerminalStatus_ReturnsTicketNotAssignable(TicketStatus status)
        {
            var ticket = new TicketEntity { Id = 1, Status = status, CreatedByUserId = 7 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new AssignTicketCommand { Id = 1, AssigneeUserId = 10 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.TicketNotAssignable, result.Error);
            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenAssigneeDoesNotExist_ReturnsInvalidAssignee()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 7 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
            _userRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((UserEntity?)null);

            var command = new AssignTicketCommand { Id = 1, AssigneeUserId = 999 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.InvalidAssignee, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenAssigneeIsNotAnAgent_ReturnsInvalidAssignee()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 7 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var notAnAgent = Agent(10);
            notAnAgent.Role = UserRole.Employee;
            _userRepository.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(notAnAgent);

            var command = new AssignTicketCommand { Id = 1, AssigneeUserId = 10 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.InvalidAssignee, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
