using System;

namespace Banking.Simulation.Core.Models.Entities;

public sealed class Organization : EntityBase<Guid>
{
    public Organization(string name, string email, string passwordHash)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
    }

    private Organization()
    {
    }

    public string Name { get; set; }
    public string Email { get; }
    public string PasswordHash { get; }
}