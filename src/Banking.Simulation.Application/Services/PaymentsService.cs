using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Simulation.Application.Extensions;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Results;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Models.Api;
using Banking.Simulation.Core.Models.Dto;
using Banking.Simulation.Core.Models.Entities;
using Banking.Simulation.Core.Models.Enums;
using Banking.Simulation.Core.Options;
using Banking.Simulation.DataAccess.Connection;
using FluentValidation;
using Kirpichyov.FriendlyJwt.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OneOf;

namespace Banking.Simulation.Application.Services;

public sealed class PaymentsService : IPaymentsService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IValidator<InitiatePaymentRequest> _initiatePaymentValidator;
    private readonly IJwtTokenReader _jwtTokenReader;
    private readonly SimulationOptions _simulationJobOptions;

    private static readonly Dictionary<string, PaymentStatus> _testBankNumbers = new()
    {
        { "1234567890", PaymentStatus.Failed },
        { "0987654321", PaymentStatus.Completed },
    };

    public PaymentsService(
        DatabaseContext databaseContext,
        IValidator<InitiatePaymentRequest> initiatePaymentValidator,
        IJwtTokenReader jwtTokenReader,
        IOptions<SimulationOptions> simulationJobOptions)
    {
        _databaseContext = databaseContext;
        _initiatePaymentValidator = initiatePaymentValidator;
        _jwtTokenReader = jwtTokenReader;
        _simulationJobOptions = simulationJobOptions.Value;
    }

    public async Task<PaymentResponse[]> GetPaymentsForCurrentOrganization(Guid? paymentId)
    {
        var organizationId = _jwtTokenReader.GetOrganizationId();

        var queryBase = _databaseContext.Payments
            .AsNoTracking()
            .Where(payment => payment.OrganizationId == organizationId);

        if (paymentId.HasValue)
        {
            queryBase = queryBase.Where(payment => payment.Id == paymentId);
        }

        var payments = await queryBase.ToArrayAsync();

        return payments.Select(ToPaymentResponse)
            .OrderByDescending(payment => payment.CreatedAtUtc)
            .ToArray();
    }

    public async Task<OneOf<Guid, ValidationFailed>> Create(InitiatePaymentRequest request)
    {
        var validationResult = await _initiatePaymentValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return new ValidationFailed(ApiErrorTypes.RequestModelValidationFailed, validationResult.Errors);
        }

        var organizationId = _jwtTokenReader.GetOrganizationId();

        var payment = new Payment(
            request.Amount,
            ToPaymentMethodDto(request.Source),
            ToPaymentMethodDto(request.Destination),
            ToPaymentCreditAllowanceDto(request.CreditAllowance),
            organizationId
        )
        {
            Comment = request.Comment,
        };

        var nextSimulationStatusAtUtc = DateTime.UtcNow.AddSeconds(Random.Shared.Next(
            _simulationJobOptions.SecondsBetweenSimulationsMin,
            _simulationJobOptions.SecondsBetweenSimulationsMax));

        payment.SetNextSimulation(PaymentStatus.Initiated, nextSimulationStatusAtUtc);
        
        if (!string.IsNullOrEmpty(request.Destination.BankAccountNumber) &&
            _testBankNumbers.TryGetValue(request.Destination.BankAccountNumber, out var status))
        {
            payment.SetNextSimulation(status, nextSimulationStatusAtUtc);
        }

        _databaseContext.Payments.Add(payment);
        await _databaseContext.SaveChangesAsync();

        return payment.Id;
    }

    private static PaymentMethodDto ToPaymentMethodDto(PaymentMethodModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new PaymentMethodDto()
        {
            CardNumber = model.CardNumber,
            BankAccountNumber = model.BankAccountNumber,
        };
    }

    private static PaymentCreditAllowanceDto ToPaymentCreditAllowanceDto(PaymentCreditAllowanceModel model)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        return new PaymentCreditAllowanceDto()
        {
            IsAllowed = model.IsAllowed,
            MaxPercent = model.MaxPercent,
            MaxPricePerMonth = model.MaxPricePerMonth,
        };
    }

    private static PaymentResponse ToPaymentResponse(Payment payment)
    {
        ArgumentNullException.ThrowIfNull(payment, nameof(payment));

        return new PaymentResponse()
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Comment = payment.Comment,
            Destination = payment.GetDestination(),
            Source = payment.GetSource(),
            CreditAllowance = payment.GetCreditAllowance(),
            Status = payment.Status,
            FailReason = payment.FailReason,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc,
        };
    }
}