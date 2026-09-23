using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.ChangeStatus;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace TicketManagement.Application.Tests.Features.Tickets.ChangeStatus
{
    public class ChangeTicketStatusCommandHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly ChangeTicketStatusCommandHandler _handler;

        public ChangeTicketStatusCommandHandlerTests()
        {
            _handler = new ChangeTicketStatusCommandHandler(_ticketRepository.Object, _unitOfWork.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenAssignedAgentMakesValidTransition_UpdatesAndReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Assigned, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new ChangeTicketStatusCommand { Id = 1, NewStatus = TicketStatus.InProgress, CurrentUserId = 10, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(TicketStatus.InProgress, ticket.Status);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenAdminMakesValidTransitionOnTicketAssignedToSomeoneElse_UpdatesAndReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.InProgress, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new ChangeTicketStatusCommand { Id = 1, NewStatus = TicketStatus.Resolved, CurrentUserId = 999, CurrentUserRole = UserRole.Admin };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(TicketStatus.Resolved, ticket.Status);
        }

        [Fact]
        public async Task HandleAsync_WhenAnyAgentRejectsAnUnassignedOpenTicket_UpdatesAndReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 7, AssignedToUserId = null };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new ChangeTicketStatusCommand { Id = 1, NewStatus = TicketStatus.Rejected, CurrentUserId = 55, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(TicketStatus.Rejected, ticket.Status);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketDoesNotExist_ReturnsNotFound()
        {
            _ticketRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TicketEntity?)null);

            var command = new ChangeTicketStatusCommand { Id = 99, NewStatus = TicketStatus.InProgress, CurrentUserId = 1, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotFound, result.Error);
        }

        [Fact]
        public async Task HandleAsync_WhenADifferentAgentTargetsAnAlreadyAssignedTicket_ReturnsNotAssignedAgent()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Assigned, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new ChangeTicketStatusCommand { Id = 1, NewStatus = TicketStatus.InProgress, CurrentUserId = 999, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotAssignedAgent, result.Error);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(TicketStatus.Open, TicketStatus.Resolved)]
        [InlineData(TicketStatus.Open, TicketStatus.InProgress)]
        [InlineData(TicketStatus.Assigned, TicketStatus.Closed)]
        [InlineData(TicketStatus.Closed, TicketStatus.Open)]
        [InlineData(TicketStatus.Rejected, TicketStatus.Open)]
        public async Task HandleAsync_WhenTransitionIsNotAllowed_ReturnsInvalidStatusTransition(TicketStatus from, TicketStatus to)
        {
            var ticket = new TicketEntity { Id = 1, Status = from, CreatedByUserId = 7, AssignedToUserId = 10 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new ChangeTicketStatusCommand { Id = 1, NewStatus = to, CurrentUserId = 10, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.InvalidStatusTransition, result.Error);
            _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
