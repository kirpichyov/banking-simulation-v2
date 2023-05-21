using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Banking.Simulation.Core.Models.Entities;
using Banking.Simulation.Core.Models.Enums;
using Banking.Simulation.Core.Models.Messaging;
using Banking.Simulation.Core.Options;
using Banking.Simulation.DataAccess.Connection;
using Bogus;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Banking.Simulation.Application.Jobs;

public sealed class SimulationJob : IJob
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<SimulationJob> _logger;
    private readonly SimulationOptions _simulationOptions;
    private readonly Faker _faker = new();
    private readonly IPublishEndpoint  _publishEndpoint;

    private static readonly string[] FailReasons =
    {
        "INSUFFICIENT_BALANCE_TO_COVER_FEE",
        "INSUFFICIENT_BALANCE",
        "CARD_INVALID",
        "CARD_EXPIRED",
        "BANK_GATEWAY_TIMEOUT",
        "USER_UNDER_SANCTIONS",
        "OPERATION_FORBIDDEN",
        "OUT_OF_LIMIT",
        "APPROVE_TIMEOUT",
        "UNEXPECTED_ERROR",
    };

    private static readonly string[] CreditFailReasons =
    {
        "OUT_OF_LIMIT",
        "DOCUMENTS_CHECK_FAILED",
        "CREDIT_HISTORY_BAD",
    };

    public SimulationJob(
        DatabaseContext databaseContext,
        ILogger<SimulationJob> logger,
        IOptions<SimulationOptions> simulationOptions, IPublishEndpoint bus)
    {
        _databaseContext = databaseContext;
        _logger = logger;
        _publishEndpoint = bus;
        _simulationOptions = simulationOptions.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var swaySeconds = -2;
        var now = DateTime.UtcNow.AddSeconds(-swaySeconds);
        
        _logger.LogInformation("Simulation job fired");

        var paymentsToSimulate = await _databaseContext.Payments
            .Include(payment => payment.Organization)
            .ThenInclude(payment => payment.WebhookConfigs)
            .Where(payment => payment.NextSimulationStatusAtUtc.HasValue &&
                              payment.NextSimulationStatusAtUtc.Value < now)
            .ToArrayAsync();

        if (paymentsToSimulate.Length == 0)
        {
            return;
        }
        
        _logger.LogInformation("Starting simulation for {PaymentsCount} payments", paymentsToSimulate.Length);

        var sendWebhookCommands = new List<SendWebhookCommand>();

        foreach (var payment in paymentsToSimulate)
        {
            var previousState = payment.Status;
            var webhookType = ApplySimulation(payment);

            var webhookConfig = payment.Organization.WebhookConfigs.FirstOrDefault(config => config.Type == webhookType);
            if (webhookConfig is not null)
            {
                sendWebhookCommands.Add(new SendWebhookCommand()
                {
                    PaymentId = payment.Id,
                    PreviousStatus = previousState,
                    CurrentStatus = payment.Status,
                    WebhookSecret = webhookConfig.Secret,
                    WebhookUrl = webhookConfig.Url,
                    WebhookType = webhookConfig.Type.ToString(),
                });
            }
        }

        await _databaseContext.SaveChangesAsync();
        await _publishEndpoint.PublishBatch(sendWebhookCommands);
    }

    private WebhookType ApplySimulation(Payment payment)
    {
        if (!payment.NextSimulationStatus.HasValue)
        {
            throw new UnreachableException("Payment expected to be non simulation completed.");
        }

        payment.SetStatus(payment.NextSimulationStatus.Value);

        switch (payment.Status)
        {
            case PaymentStatus.Initiated:
                HandleInitiated(payment);
                return WebhookType.PaymentInitiated;
            case PaymentStatus.Failed:
                HandleFailed(payment);
                var isCreditError = CreditFailReasons.Contains(payment.FailReason);
                return isCreditError ? WebhookType.CreditRequestFailed : WebhookType.PaymentFailed;
            case PaymentStatus.CreditRequestInitiated:
                HandleCreditRequestInitiated(payment);
                return WebhookType.CreditRequestInitiated;
            case PaymentStatus.Completed:
                HandleCompleted(payment);
                return WebhookType.PaymentCompleted;
            case PaymentStatus.Created:
            default:
                throw new ArgumentException($"Value '{payment.Status}' is unexpected.");
        }
    }

    private void HandleInitiated(Payment payment)
    {
        var isFailed = IsChanceHit(_simulationOptions.AfterInitiateFailChance);
        var nextDate = GetNextSimulationAtUtc(DateTime.UtcNow);
            
        if (isFailed)
        {
            var failReason = _faker.PickRandom(FailReasons);
            payment.SetNextSimulation(PaymentStatus.Failed, nextDate, failReason);
            return;
        }

        if (payment.GetCreditAllowance().IsAllowed)
        {
            payment.SetNextSimulation(PaymentStatus.CreditRequestInitiated, nextDate);
        }
        else
        {
            payment.SetNextSimulation(PaymentStatus.Completed, nextDate);
        }
    }

    private void HandleFailed(Payment payment)
    {
        payment.FinishSimulation(payment.NextSimulationFailReason);
    }
    
    private void HandleCompleted(Payment payment)
    {
        var bankFee = _faker.Finance.Amount(min: 0.1m, max: 20m);
        payment.FinishSimulation(null, bankFee);
    }

    private void HandleCreditRequestInitiated(Payment payment)
    {
        var isFailed = IsChanceHit(_simulationOptions.CreditApprovalFailChance);
        var nextDate = GetNextSimulationAtUtc(DateTime.UtcNow);
        
        if (isFailed)
        {
            var failReason = _faker.PickRandom(CreditFailReasons);
            payment.SetNextSimulation(PaymentStatus.Failed, nextDate, failReason);
            return;
        }
        
        payment.SetNextSimulation(PaymentStatus.Completed, nextDate);
    }

    private bool IsChanceHit(int chance)
    {
        if (chance >= 100)
        {
            return true;
        }
        
        if (chance <= 0f)
        {
            return false;
        }
        
        var random = Random.Shared.Next(0, 100);
        return random < chance;
    }

    private DateTime GetNextSimulationAtUtc(DateTime referenceDate)
    {
        var seconds = Random.Shared.Next(
            _simulationOptions.SecondsBetweenSimulationsMin,
            _simulationOptions.SecondsBetweenSimulationsMax);
        
        return referenceDate.AddSeconds(seconds);
    }
}