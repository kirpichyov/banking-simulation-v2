using System.Linq;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Simulation.Api.Controllers.v1;

[ApiVersion("1.0")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register-organization")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrganizationCreatedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] CreateOrganizationRequest request)
    {
        var result = await _authService.CreateOrganization(request);

        return result.Match(
            organizationId => StatusCode(StatusCodes.Status201Created, new OrganizationCreatedResponse(organizationId)),
            validationFailed => BadRequest(new ApiErrorResponse(validationFailed.ErrorType, validationFailed.Errors)));
    }

    [HttpPost("obtain-jwt")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwtResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtainJwt([FromBody] SignInRequest request)
    {
        var result = await _authService.GenerateJwt(request);

        return result.Match<IActionResult>(
            jwtResponse => Ok(jwtResponse),
            validationFailed => BadRequest(new ApiErrorResponse(validationFailed.ErrorType, validationFailed.Errors)));
    }
}