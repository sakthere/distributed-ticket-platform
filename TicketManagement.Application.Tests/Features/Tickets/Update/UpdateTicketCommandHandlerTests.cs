using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.Update;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace TicketManagement.Application.Tests.Features.Tickets.Update
{
    public class UpdateTicketCommandHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly UpdateTicketCommandHandler _handler;

        public UpdateTicketCommandHandlerTests()
        {
            _handler = new UpdateTicketCommandHandler(_ticketRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenOwnerUpdatesOpenTicket_UpdatesAndReturnsSuccess()
        {
            var ticket = new TicketEntity
            {
                Id = 1,
                Title = "Old title",
                Description = "Old description",
                Status = TicketStatus.Open,
                CreatedByUserId = 42
            };

            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new UpdateTicketCommand
            {
                Id = 1,
                Title = "New title",
                Description = "New description",
                CurrentUserId = 42
            };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal("New title", result.Value.Title);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketDoesNotExist_ReturnsNotFound()
        {
            _ticketRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TicketEntity?)null);

            var command = new UpdateTicketCommand { Id = 99, Title = "x", Description = "y", CurrentUserId = 1 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotFound, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenCallerIsNotOwner_ReturnsNotTicketOwner()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.Open, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new UpdateTicketCommand { Id = 1, Title = "x", Description = "y", CurrentUserId = 999 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotTicketOwner, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketIsNotOpen_ReturnsTicketNotEditable()
        {
            var ticket = new TicketEntity { Id = 1, Status = TicketStatus.InProgress, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var command = new UpdateTicketCommand { Id = 1, Title = "x", Description = "y", CurrentUserId = 42 };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.TicketNotEditable, result.Error);
            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
