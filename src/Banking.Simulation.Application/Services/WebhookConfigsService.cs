using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Simulation.Application.Extensions;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Results;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Models.Api;
using Banking.Simulation.Core.Models.Entities;
using Banking.Simulation.DataAccess.Connection;
using FluentValidation;
using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;

namespace Banking.Simulation.Application.Services;

public sealed class WebhookConfigsService : IWebhookConfigsService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IJwtTokenReader _jwtTokenReader;
    private readonly IValidator<CreateWebhookConfigRequest> _createModelValidator;

    public WebhookConfigsService(DatabaseContext databaseContext,
        IJwtTokenReader jwtTokenReader,
        IValidator<CreateWebhookConfigRequest> createModelValidator)
    {
        _databaseContext = databaseContext;
        _jwtTokenReader = jwtTokenReader;
        _createModelValidator = createModelValidator;
    }

    public async Task<WebhookConfigResponse[]> GetCurrentForCurrentOrganization()
    {
        var organizationId = _jwtTokenReader.GetOrganizationId();

        var configs = await _databaseContext.WebhookConfigs
            .AsNoTracking()
            .Where(config => config.OrganizationId == organizationId)
            .ToArrayAsync();

        return configs.Select(ToWebhookConfigResponse).ToArray();
    }

    public async Task<OneOf<WebhookConfigResponse, ValidationFailed, AlreadyExists>> Create(CreateWebhookConfigRequest request)
    {
        var validationResult = await _createModelValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return new ValidationFailed(ApiErrorTypes.RequestModelValidationFailed, validationResult.Errors);
        }

        var organizationId = _jwtTokenReader.GetOrganizationId();

        var isUrlInUseForType = await _databaseContext.WebhookConfigs
            .AnyAsync(webhookConfig => webhookConfig.OrganizationId == organizationId &&
                                       webhookConfig.Url.ToLower() == request.Url.ToLower() &&
                                       webhookConfig.Type == request.Type);

        if (isUrlInUseForType)
        {
            return new AlreadyExists(ApiErrorTypes.RequestValidationFailed,
                $"Webhook config for type '{request.Type}' already exists for url '{request.Url}'.");
        }
        
        var webhookConfig = new WebhookConfig(request.Type, request.Url, request.Secret, organizationId);
        
        _databaseContext.WebhookConfigs.Add(webhookConfig);
        await _databaseContext.SaveChangesAsync();

        return ToWebhookConfigResponse(webhookConfig);
    }

    public async Task<OneOf<Success, NotFound>> Delete(Guid id)
    {
        var organizationId = _jwtTokenReader.GetOrganizationId();

        var config = await _databaseContext.WebhookConfigs
            .FirstOrDefaultAsync(config => config.Id == id && config.OrganizationId == organizationId);

        if (config is null)
        {
            return new NotFound();
        }

        _databaseContext.WebhookConfigs.Remove(config);
        await _databaseContext.SaveChangesAsync();

        return new Success();
    }

    private static WebhookConfigResponse ToWebhookConfigResponse(WebhookConfig webhookConfig)
    {
        return new WebhookConfigResponse()
        {
            Id = webhookConfig.Id,
            Type = webhookConfig.Type,
            Url = webhookConfig.Url,
        };
    }
}