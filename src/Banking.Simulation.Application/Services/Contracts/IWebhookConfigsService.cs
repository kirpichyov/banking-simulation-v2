using System;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Results;
using OneOf;
using OneOf.Types;

namespace Banking.Simulation.Application.Services.Contracts;

public interface IWebhookConfigsService
{
    Task<WebhookConfigResponse[]> GetCurrentForCurrentOrganization();
    Task<OneOf<WebhookConfigResponse, ValidationFailed, AlreadyExists>> Create(CreateWebhookConfigRequest request);
    Task<OneOf<Success, NotFound>> Delete(Guid id);
}