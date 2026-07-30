using Moq;
using TicketManagement.Application.Features.Tickets.Create;
using TicketManagement.Application.Interfaces;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Enums;

namespace TicketManagement.Application.Tests.Features.Tickets.Create
{
    public class CreateTicketCommandHandlerTests
    {
        private readonly Mock<ITicketRepository> _ticketRepository = new();
        private readonly CreateTicketCommandHandler _handler;
        public CreateTicketCommandHandlerTests()
        {
            _handler = new CreateTicketCommandHandler(_ticketRepository.Object);
        }

        [Fact]
        public async Task HandleAsync_WithValidCommand_PersistsTicketAndReturnsMappedResult()
        {
            var command = new CreateTicketCommand
            {
                Title = "Printer not working",
                Description = "The office printer on the 3rd floor is jammed.",
                Impact = TicketImpact.Medium,
                Urgency = TicketUrgency.Low,
                CreatedByUserId = 12
            };

            var result = await _handler.HandleAsync(command);

            Assert.True(result.IsSuccess);
            Assert.Equal(command.Title, result.Value.Title);
            Assert.Equal(command.Description, result.Value.Description);
            Assert.Equal(TicketStatus.Open, result.Value.Status);
            Assert.Equal(TicketPriority.Low, result.Value.Priority);

            _ticketRepository.Verify(r => r.AddAsync(It.Is<Ticket>(t => t.Title == command.Title && t.CreatedByUserId == command.CreatedByUserId && t.Status == TicketStatus.Open)), Times.Once);

            _ticketRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Theory]
        [InlineData(TicketImpact.High, TicketUrgency.High, TicketPriority.Critical)]
        [InlineData(TicketImpact.High, TicketUrgency.Medium, TicketPriority.High)]
        [InlineData(TicketImpact.Medium, TicketUrgency.Medium, TicketPriority.Medium)]
        [InlineData(TicketImpact.Low, TicketUrgency.Low, TicketPriority.Low)]
        [InlineData(TicketImpact.High, TicketUrgency.Low, TicketPriority.Medium)]
        public async Task HandleAsync_CalculatesPriorityAccordingToPolicy(TicketImpact impact, TicketUrgency urgency, TicketPriority expectedPriority)
        {
            var command = new CreateTicketCommand
            {
                Title = "Printer not working",
                Description = "The office printer on the 3rd floor is jammed.",
                Impact = impact,
                Urgency = urgency,
                CreatedByUserId = 12
            };

            var result = await _handler.HandleAsync(command);

            Assert.Equal(expectedPriority, result.Value.Priority);
        }
    }
}
