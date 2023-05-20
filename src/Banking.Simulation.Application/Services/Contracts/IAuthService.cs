using System;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Results;
using OneOf;

namespace Banking.Simulation.Application.Services.Contracts;

public interface IAuthService
{
    Task<OneOf<Guid, ValidationFailed>> CreateOrganization(CreateOrganizationRequest request);
    Task<OneOf<JwtResponse, ValidationFailed>> GenerateJwt(SignInRequest request);
}