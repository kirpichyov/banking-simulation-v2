using Banking.Simulation.Application.Models;
using FluentValidation;

namespace Banking.Simulation.Application.Validators;

public sealed class PaymentMethodModelValidator : AbstractValidator<PaymentMethodModel>
{
    public PaymentMethodModelValidator()
    {
        RuleFor(model => model.CardNumber)
            .Empty()
            .When(model => !string.IsNullOrEmpty(model.BankAccountNumber));
        
        RuleFor(model => model.CardNumber)
            .NotEmpty()
            .When(model => string.IsNullOrEmpty(model.BankAccountNumber));

        RuleFor(model => model.BankAccountNumber)
            .NotEmpty()
            .When(model => string.IsNullOrEmpty(model.CardNumber));
        
        When(model => string.IsNullOrEmpty(model.CardNumber), () =>
        {
            RuleFor(model => model.BankAccountNumber)
                .NotEmpty()
                .MaximumLength(64);
        });
        
        When(model => string.IsNullOrEmpty(model.BankAccountNumber), () =>
        {
            RuleFor(model => model.CardNumber)
                .NotEmpty()
                .MaximumLength(64);
        });
    }
}