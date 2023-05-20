using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Banking.Simulation.Core.Models.Dto;
using Banking.Simulation.Core.Models.Enums;

namespace Banking.Simulation.Core.Models.Entities;

public sealed class Payment : EntityBase<Guid>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    
    public Payment(
        decimal amount,
        PaymentMethodDto source,
        PaymentMethodDto destination,
        PaymentCreditAllowanceDto creditAllowance,
        Guid organizationId)
    {
        Amount = amount;
        Status = PaymentStatus.Created;
        SourceJson = JsonSerializer.Serialize(source, JsonSerializerOptions);
        DestinationJson = JsonSerializer.Serialize(destination, JsonSerializerOptions);
        CreditAllowanceJson = JsonSerializer.Serialize(creditAllowance, JsonSerializerOptions);
        OrganizationId = organizationId;
        CreatedAtUtc = DateTime.UtcNow;
    }

    private Payment()
    {
    }

    public decimal Amount { get; }
    public string SourceJson { get; }
    public string DestinationJson { get; }
    public string CreditAllowanceJson { get; }
    public string Comment { get; init; }
    public decimal? BankFee { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentStatus? NextSimulationStatus { get; private set; }
    public string FailReason { get; private set; }
    public string NextSimulationFailReason { get; private set; }
    public DateTime CreatedAtUtc { get; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? NextSimulationStatusAtUtc { get; private set; }
    public Guid OrganizationId { get; }
    public Organization Organization { get; }

    public void SetNextSimulation(PaymentStatus nextStatus, DateTime nextStatusAtUtc, string nextFailReason = null)
    {
        NextSimulationStatus = nextStatus;
        NextSimulationStatusAtUtc = nextStatusAtUtc;
        NextSimulationFailReason = nextFailReason;
    }
    
    public void FinishSimulation(string failReason =  null, decimal? bankFee = null)
    {
        NextSimulationStatus = null;
        NextSimulationStatusAtUtc = null;
        NextSimulationFailReason = null;
        FailReason = failReason;
        BankFee = bankFee;
    }

    public void SetStatus(PaymentStatus status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }
    
    public PaymentMethodDto GetSource()
    {
        return JsonSerializer.Deserialize<PaymentMethodDto>(SourceJson, JsonSerializerOptions);
    }
    
    public PaymentMethodDto GetDestination()
    {
        return JsonSerializer.Deserialize<PaymentMethodDto>(DestinationJson, JsonSerializerOptions);
    }
    
    public PaymentCreditAllowanceDto GetCreditAllowance()
    {
        return JsonSerializer.Deserialize<PaymentCreditAllowanceDto>(CreditAllowanceJson, JsonSerializerOptions);
    }
}