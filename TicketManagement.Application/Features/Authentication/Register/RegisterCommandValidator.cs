using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace TicketManagement.Application.Features.Authentication.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required.").MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is required.").MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().MaximumLength(225);
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.").MinimumLength(6).MaximumLength(100);

        }
    }
}
