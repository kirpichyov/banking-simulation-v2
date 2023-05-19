namespace Banking.Simulation.Application.Models;

public sealed record SignInRequest
{
    public string Email { get; init; }
    public string Password { get; init; }
}