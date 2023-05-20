using Banking.Simulation.Application.Models;
using FluentValidation;

namespace Banking.Simulation.Application.Validators;

public sealed class InitiatePaymentRequestValidator : AbstractValidator<InitiatePaymentRequest>
{
    public InitiatePaymentRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(model => model.Source)
            .NotEmpty();
        
        RuleFor(model => model.Destination)
            .NotEmpty();

        RuleFor(model => model.Source)
            .SetValidator(new PaymentMethodModelValidator());

        RuleFor(model => model.Destination)
            .SetValidator(new PaymentMethodModelValidator());

        RuleFor(model => model.CreditAllowance)
            .NotEmpty();

        RuleFor(model => model.Amount)
            .NotEmpty()
            .GreaterThan(0m);

        RuleFor(model => model.Comment)
            .MaximumLength(128);

        When(model => model.CreditAllowance.IsAllowed, () =>
        {
            RuleFor(model => model.CreditAllowance.MaxPricePerMonth)
                .NotEmpty()
                .GreaterThan(0m);
        
            RuleFor(model => model.CreditAllowance.MaxPercent)
                .NotEmpty()
                .GreaterThan(0f)
                .LessThanOrEqualTo(1f);
        });
    }
}