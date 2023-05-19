using System;
using System.Threading.Tasks;
using Banking.Simulation.Application.Models;
using Banking.Simulation.Application.Results;
using Banking.Simulation.Application.Services.Contracts;
using Banking.Simulation.Core.Models.Api;
using Banking.Simulation.Core.Models.Entities;
using Banking.Simulation.Core.Options;
using Banking.Simulation.DataAccess.Connection;
using FluentValidation;
using FluentValidation.Results;
using Kirpichyov.FriendlyJwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OneOf;

namespace Banking.Simulation.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly DatabaseContext _databaseContext;
    private readonly AuthOptions _authOptions;
    private readonly IValidator<SignInRequest> _signInRequestValidator;
    private readonly IValidator<CreateOrganizationRequest> _createOrganizationRequestValidator;

    public AuthService(DatabaseContext databaseContext,
        IOptions<AuthOptions> authOptions,
        IValidator<SignInRequest> signInRequestValidator,
        IValidator<CreateOrganizationRequest> createOrganizationRequestValidator)
    {
        _databaseContext = databaseContext;
        _authOptions = authOptions.Value;
        _signInRequestValidator = signInRequestValidator;
        _createOrganizationRequestValidator = createOrganizationRequestValidator;
    }
    
    public async Task<OneOf<Guid, ValidationFailed>> CreateOrganization(CreateOrganizationRequest request)
    {
        var validationResult = await _createOrganizationRequestValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return new ValidationFailed(ApiErrorTypes.RequestModelValidationFailed, validationResult.Errors);
        }

        var isEmailInUse = await _databaseContext.Organizations
            .AsNoTracking()
            .AnyAsync(organization => organization.Email == request.Email);

        if (isEmailInUse)
        {
            return new ValidationFailed(ApiErrorTypes.RequestValidationFailed,
                new ValidationFailure(nameof(request.Email), "Email is already in use."));
        }

        var password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var organization = new Organization(request.Name, request.Email, password);

        _databaseContext.Organizations.Add(organization);
        await _databaseContext.SaveChangesAsync();

        return organization.Id;
    }

    public async Task<OneOf<JwtResponse, ValidationFailed>> GenerateJwt(SignInRequest request)
    {
        var validationResult = await _signInRequestValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            return new ValidationFailed(ApiErrorTypes.RequestModelValidationFailed, validationResult.Errors);
        }

        var organization = await _databaseContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(organization => organization.Email == request.Email);

        if (organization is null || !BCrypt.Net.BCrypt.Verify(request.Password, organization.PasswordHash))
        {
            return new ValidationFailed(ApiErrorTypes.RequestValidationFailed,
                new ValidationFailure(null, "Credentials are invalid."));
        }
        
        return GenerateJwt(organization);
    }

    private JwtResponse GenerateJwt(Organization organization)
    {
        var lifeTime = TimeSpan.FromSeconds(_authOptions.AccessTokenLifetime);

        var jwt = new JwtTokenBuilder(lifeTime, _authOptions.Secret)
            .WithAudience(_authOptions.Audience)
            .WithIssuer(_authOptions.Issuer)
            .WithPayloadData("organization_id", organization.Id.ToString())
            .Build();

        return new JwtResponse()
        {
            AccessToken = jwt.Token,
            ExpiresAtUtc = jwt.ExpiresOn,
        };
    }
}