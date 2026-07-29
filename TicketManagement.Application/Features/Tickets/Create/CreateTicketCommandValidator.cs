

using FluentValidation;

namespace TicketManagement.Application.Features.Tickets.Create
{
    public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Impact).IsInEnum();
            RuleFor(x => x.Urgency).IsInEnum();
        }
    }
}
