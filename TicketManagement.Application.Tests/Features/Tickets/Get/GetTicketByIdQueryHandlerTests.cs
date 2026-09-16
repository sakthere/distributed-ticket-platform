using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.Get;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace TicketManagement.Application.Tests.Features.Tickets.Get
{
    public class GetTicketByIdQueryHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly GetTicketByIdQueryHandler _handler;

        public GetTicketByIdQueryHandlerTests()
        {
            _handler = new GetTicketByIdQueryHandler(_ticketRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenEmployeeViewsOwnTicket_ReturnsSuccess()
        {
            var ticket = new TicketEntity { Id = 1, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var query = new GetTicketByIdQuery { Id = 1, CurrentUserId = 42, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.Id);
        }

        [Theory]
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.Agent)]
        public async Task HandleAsync_WhenAdminOrAgentViewsAnyTicket_ReturnsSuccess(UserRole role)
        {
            var ticket = new TicketEntity { Id = 1, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var query = new GetTicketByIdQuery { Id = 1, CurrentUserId = 999, CurrentUserRole = role };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HandleAsync_WhenEmployeeViewsSomeoneElsesTicket_ReturnsNotTicketOwner()
        {
            var ticket = new TicketEntity { Id = 1, CreatedByUserId = 42 };
            _ticketRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

            var query = new GetTicketByIdQuery { Id = 1, CurrentUserId = 999, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotTicketOwner, result.Error);
        }

        [Fact]
        public async Task HandleAsync_WhenTicketDoesNotExist_ReturnsNotFound()
        {
            _ticketRepository.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TicketEntity?)null);

            var query = new GetTicketByIdQuery { Id = 99, CurrentUserId = 1, CurrentUserRole = UserRole.Employee };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.NotFound, result.Error);
        }
    }
}
