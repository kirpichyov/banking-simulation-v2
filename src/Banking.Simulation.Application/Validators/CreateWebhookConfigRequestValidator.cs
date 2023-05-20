using System;
using Banking.Simulation.Application.Models;
using FluentValidation;

namespace Banking.Simulation.Application.Validators;

public sealed class CreateWebhookConfigRequestValidator : AbstractValidator<CreateWebhookConfigRequest>
{
    public CreateWebhookConfigRequestValidator()
    {
        RuleFor(model => model.Secret).NotEmpty().MaximumLength(256);
        RuleFor(model => model.Type).IsInEnum();

        RuleFor(model => model.Url)
            .NotEmpty()
            .MaximumLength(128)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult)
                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Url has invalid format.");
    }
}