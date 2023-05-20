using System;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Simulation.Api.Controllers.v1;

public sealed class PaymentsController : ApiControllerBase
{
    private readonly IPaymentsService _paymentsService;

    public PaymentsController(IPaymentsService paymentsService)
    {
        _paymentsService = paymentsService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaymentResponse[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments([FromQuery] Guid? paymentId)
    {
        var payments = await _paymentsService.GetPaymentsForCurrentOrganization(paymentId);
        return Ok(payments);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] InitiatePaymentRequest request)
    {
        var result = await _paymentsService.Create(request);

        return result.Match(
            paymentId => StatusCode(StatusCodes.Status201Created, new { paymentId }),
            validationFailed => BadRequest(new ApiErrorResponse(validationFailed.ErrorType, validationFailed.Errors)));
    }
}