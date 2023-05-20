using System;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Simulation.Api.Controllers.v1;

[ApiVersion("1.0")]
public sealed class WebhookConfigsController : ApiControllerBase
{
    private readonly IWebhookConfigsService _webhookConfigsService;

    public WebhookConfigsController(IWebhookConfigsService webhookConfigsService)
    {
        _webhookConfigsService = webhookConfigsService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(WebhookConfigResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var configs = await _webhookConfigsService.GetCurrentForCurrentOrganization();
        return Ok(configs);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(WebhookConfigResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWebhookConfigRequest request)
    {
        var result = await _webhookConfigsService.Create(request);

        return result.Match(
            config => StatusCode(StatusCodes.Status201Created, config),
            validationFailed => BadRequest(new ApiErrorResponse(validationFailed.ErrorType, validationFailed.Errors)),
            alreadyExists => Conflict(new ApiErrorResponse(alreadyExists.ErrorType, alreadyExists.ErrorMessage)));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute(Name = "id")] Guid webhookId)
    {
        var result = await _webhookConfigsService.Delete(webhookId);

        return result.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound(new ApiErrorResponse(ApiErrorTypes.RequestValidationFailed, "Webhook config is not found.")));
    }
}