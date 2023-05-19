using System;

namespace Banking.Simulation.Application.Models;

public sealed record JwtResponse
{
    public string AccessToken { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}