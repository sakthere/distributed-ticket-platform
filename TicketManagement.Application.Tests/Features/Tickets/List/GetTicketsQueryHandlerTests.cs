using System.Threading;
using Moq;
using TicketManagement.Application.Features.Tickets;
using TicketManagement.Application.Features.Tickets.List;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Enums;
using Xunit;
using TicketEntity = TicketManagement.Domain.Entities.Ticket;

namespace TicketManagement.Application.Tests.Features.Tickets.List
{
    public class GetTicketsQueryHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly GetTicketsQueryHandler _handler;

        public GetTicketsQueryHandlerTests()
        {
            _handler = new GetTicketsQueryHandler(_ticketRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WhenEmployeeRequestsList_ForcesCreatedByFilterToSelf_RegardlessOfWhatWasRequested()
        {
            TicketListFilter? capturedFilter = null;
            _ticketRepository
                .Setup(r => r.GetPagedAsync(It.IsAny<TicketListFilter>(), It.IsAny<CancellationToken>()))
                .Callback<TicketListFilter, CancellationToken>((f, _) => capturedFilter = f)
                .ReturnsAsync((new List<TicketEntity>(), 0));

            // Employee tries to ask for someone else's tickets (CreatedByUserId = 999) -
            // the handler must ignore that and force it back to their own id.
            var query = new GetTicketsQuery { CurrentUserId = 42, CurrentUserRole = UserRole.Employee, CreatedByUserId = 999 };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsSuccess);
            Assert.NotNull(capturedFilter);
            Assert.Equal(42, capturedFilter!.CreatedByUserId);
        }

        [Fact]
        public async Task HandleAsync_WhenAgentRequestsList_DoesNotForceACreatedByFilter()
        {
            TicketListFilter? capturedFilter = null;
            _ticketRepository
                .Setup(r => r.GetPagedAsync(It.IsAny<TicketListFilter>(), It.IsAny<CancellationToken>()))
                .Callback<TicketListFilter, CancellationToken>((f, _) => capturedFilter = f)
                .ReturnsAsync((new List<TicketEntity>(), 0));

            var query = new GetTicketsQuery { CurrentUserId = 10, CurrentUserRole = UserRole.Agent };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsSuccess);
            Assert.Null(capturedFilter!.CreatedByUserId);
        }

        [Fact]
        public async Task HandleAsync_MapsRepositoryResultsIntoPagedResult()
        {
            // Priority has a private setter (see Sprint 4 - it can only change via
            // RecalculatePriority(), so an invalid Impact/Urgency/Priority combination
            // is unrepresentable). Set Impact/Urgency and let the entity derive it,
            // rather than trying to assign Priority directly.
            var ticketA = new TicketEntity { Id = 1, Title = "A", Status = TicketStatus.Open, CreatedByUserId = 42, Impact = TicketImpact.Low, Urgency = TicketUrgency.Low };
            ticketA.RecalculatePriority();
            var ticketB = new TicketEntity { Id = 2, Title = "B", Status = TicketStatus.Assigned, CreatedByUserId = 42, AssignedToUserId = 10, Impact = TicketImpact.High, Urgency = TicketUrgency.High };
            ticketB.RecalculatePriority();

            var tickets = new List<TicketEntity> { ticketA, ticketB };
            _ticketRepository
                .Setup(r => r.GetPagedAsync(It.IsAny<TicketListFilter>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((tickets, 2));

            var query = new GetTicketsQuery { CurrentUserId = 42, CurrentUserRole = UserRole.Employee, Page = 1, PageSize = 20 };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.TotalCount);
            Assert.Equal(2, result.Value.Items.Count);
            Assert.Equal("A", result.Value.Items[0].Title);
            Assert.Equal(TicketPriority.Low, result.Value.Items[0].Priority);
            Assert.Equal(10, result.Value.Items[1].AssignedToUserId);
            Assert.Equal(TicketPriority.Critical, result.Value.Items[1].Priority);
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        [InlineData(-1, 20)]
        public async Task HandleAsync_WhenPaginationIsOutOfRange_ReturnsInvalidPagination(int page, int pageSize)
        {
            var query = new GetTicketsQuery { CurrentUserId = 1, CurrentUserRole = UserRole.Admin, Page = page, PageSize = pageSize };

            var result = await _handler.HandleAsync(query);

            Assert.True(result.IsFailure);
            Assert.Equal(TicketErrors.InvalidPagination, result.Error);
            _ticketRepository.Verify(r => r.GetPagedAsync(It.IsAny<TicketListFilter>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
