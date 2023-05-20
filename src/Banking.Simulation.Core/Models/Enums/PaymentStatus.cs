using NetEscapades.EnumGenerators;

namespace Banking.Simulation.Core.Models.Enums;

[EnumExtensions]
public enum PaymentStatus
{
    Created,
    Initiated,
    CreditRequestInitiated,
    Completed,
    Failed,
}