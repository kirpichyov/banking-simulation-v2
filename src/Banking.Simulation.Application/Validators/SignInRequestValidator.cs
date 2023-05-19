using Banking.Simulation.Application.Models;
using FluentValidation;

namespace Banking.Simulation.Application.Validators;

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(model => model.Email).NotEmpty();
        RuleFor(model => model.Password).NotEmpty();
    }
}