using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.Delete;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace TicketManagement.Application.Tests.Features.Tickets.Delete
{
    public class DeleteTicketCommandHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly DeleteTicketCommandHandler _handler;

        public DeleteTicketCommandHandlerTests()
        {
            _handler = new DeleteTicketCommandHandler(_ticketRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenOwnerDeletesOpenTicket_SoftDeletesAndReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new DeleteTicketCommand { Id = 1, CurrentUserId = 42, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.True(ticket.IsDeleted);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenAdminDeletesTicketInAnyStatus_SoftDeletesAndReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.InProgress, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new DeleteTicketCommand { Id = 1, CurrentUserId = 999, CurrentUserRole = UserRole.Admin };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.True(ticket.IsDeleted);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketDoesNotExist_ReturnsNotFound()
        {
            _ticketRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TicketEntity?)null);

            var command = new DeleteTicketCommand { Id = 99, CurrentUserId = 1, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotFound, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenCallerIsNeitherOwnerNorAdmin_ReturnsNotTicketOwner()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new DeleteTicketCommand { Id = 1, CurrentUserId = 999, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotTicketOwner, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenOwnerDeletesNonOpenTicket_ReturnsTicketNotDeletable()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.InProgress, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new DeleteTicketCommand { Id = 1, CurrentUserId = 42, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.TicketNotDeletable, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
