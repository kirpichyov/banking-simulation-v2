using System.Collections.Generic;
using FluentValidation.Results;

namespace Banking.Simulation.Application.Results;

public sealed record ValidationFailed(string ErrorType, IEnumerable<ValidationFailure> Errors)
{
    public ValidationFailed(string errorType, ValidationFailure error) : this(errorType, new[] { error })
    {
    }
}