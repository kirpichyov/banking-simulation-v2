using System.Net.Mail;
using Banking.Simulation.Application.Extensions;
using Banking.Simulation.Application.Models;
using FluentValidation;

namespace Banking.Simulation.Application.Validators;

public sealed class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(model => model.Name)
            .NotEmpty()
            .MaximumLength(32);
        
        RuleFor(model => model.Email)
            .NotEmpty()
            .Must(email => MailAddress.TryCreate(email, out _))
            .WithMessage("Has invalid format.");
        
        RuleFor(model => model.Password).ApplyPasswordValidationRules();
    }
}