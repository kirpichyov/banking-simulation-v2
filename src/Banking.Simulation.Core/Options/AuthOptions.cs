namespace Banking.Simulation.Core.Options;

public sealed class AuthOptions
{
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public int AccessTokenLifetime { get; init; }
    public string Secret { get; init; }
}