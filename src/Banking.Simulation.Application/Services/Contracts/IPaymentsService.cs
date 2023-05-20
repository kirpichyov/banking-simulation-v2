using System;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Results;
using OneOf;

namespace Banking.Simulation.Application.Services.Contracts;

public interface IPaymentsService
{
    Task<PaymentResponse[]> GetPaymentsForCurrentOrganization(Guid? paymentId);
    Task<OneOf<Guid, ValidationFailed>> Create(InitiatePaymentRequest request);
}