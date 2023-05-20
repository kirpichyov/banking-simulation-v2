using System;
using Kirpichyov.FriendlyJwt.Contracts;

namespace Banking.Simulation.Application.Extensions;

public static class JwtTokenReaderExtensions
{
    public static Guid GetOrganizationId(this IJwtTokenReader reader)
    {
        var value = reader["organization_id"];
        return Guid.Parse(value);
    }
}