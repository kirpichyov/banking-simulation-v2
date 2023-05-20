using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Banking.Simulation.Application.Results;

public sealed record ValidationFailed
{
    public ValidationFailed(string errorType, IEnumerable<ValidationFailure> errors)
    {
        ErrorType = errorType;
        Errors = errors.ToArray();
    }
    
    public ValidationFailed(string errorType, ValidationFailure error) : this(errorType, new[] { error })
    {
    }

    public string ErrorType { get; }
    public ValidationFailure[] Errors { get; }
}