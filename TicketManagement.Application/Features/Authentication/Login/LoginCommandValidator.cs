using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace TicketManagement.Application.Features.Authentication.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().
                WithMessage("Please enter email in correct format").MaximumLength(225);
            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}
