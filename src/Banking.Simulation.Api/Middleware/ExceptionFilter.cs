using Banking.Simulation.Core.Models.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Banking.Simulation.Api.Middleware;

internal sealed class ExceptionFilter : ExceptionFilterAttribute
{
	private readonly ILogger<ExceptionFilter> _logger;
	private readonly IHostEnvironment _environment;
	
	public ExceptionFilter(ILogger<ExceptionFilter> logger, IHostEnvironment environment)
	{
		_logger = logger;
		_environment = environment;
	}
	
	public override void OnException(ExceptionContext context)
	{
		HandleExceptionAsync(context);
		context.ExceptionHandled = true;
	}

	private void HandleExceptionAsync(ExceptionContext context)
	{
		switch (context.Exception)
		{
			default:
				_logger.LogError(context.Exception, "Unexpected error occured during request");
				SetInternalServerErrorExceptionResult(context);
				break;
		}
	}

	private void SetInternalServerErrorExceptionResult(ExceptionContext context)
	{
		var propertyError = new ApiErrorResponseNode(null, "Unexpected error occured.");
		var responseModel = new ApiErrorResponse(ApiErrorTypes.Generic, propertyError);

		if (!_environment.IsProduction())
		{
			responseModel = new ApiErrorResponseWithException(responseModel, context.Exception);
		}

		context.Result = new JsonResult(responseModel)
		{
			StatusCode = 500
		};
	}
}
