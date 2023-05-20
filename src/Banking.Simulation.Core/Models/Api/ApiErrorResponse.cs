using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace Banking.Simulation.Core.Models.Api;

public class ApiErrorResponse
{
    public ApiErrorResponse(string errorType, ApiErrorResponseNode[] errorNodes)
    {
        ErrorType = errorType;
        Errors = errorNodes;
    }

    public ApiErrorResponse(string errorType, ApiErrorResponseNode errorNode)
    {
        ErrorType = errorType;
        Errors = new[] { errorNode };
    }

    public ApiErrorResponse(string errorType, ValidationFailure[] failures)
    {
        var nodes = failures.GroupBy(
            failure => failure.PropertyName,
            failure => failure.ErrorMessage,
            (propertyName, propertyErrors) => new ApiErrorResponseNode(propertyName, propertyErrors.ToArray()));

        ErrorType = errorType;
        Errors = nodes.ToArray();
    }
    
    public ApiErrorResponse(string errorType, string errorMessage)
    {
        ErrorType = errorType;
        Errors = new[] { new ApiErrorResponseNode(null, errorMessage) };
    }
    
    public string ErrorType { get; init; }
    public ApiErrorResponseNode[] Errors { get; init; }
}