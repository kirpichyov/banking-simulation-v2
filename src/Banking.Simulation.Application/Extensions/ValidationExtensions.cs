using System.Text.RegularExpressions;
using FluentValidation;

namespace Banking.Simulation.Application.Extensions;

internal static class ValidationExtensions
{
    public static void ApplyPasswordValidationRules<TModel>(this IRuleBuilderInitial<TModel, string> initial)
    {
        initial.Cascade(CascadeMode.Stop);
        
        initial
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(32);

        initial
            .Must(password => !password.Contains(' '))
            .WithMessage("Can't contain a whitespace")
            .Must(password => Regex.IsMatch(password, @"(?=.*[\W_])"))
            .WithMessage("Must have at least 1 special character")
            .Must(password => Regex.IsMatch(password, @"(?=.*\d)"))
            .WithMessage("Must have at least 1 number")
            .Must(password => Regex.IsMatch(password, @"(?=.*[A-Z])"))
            .WithMessage("Must have at least 1 upper case character");
    }
}