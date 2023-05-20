using NetEscapades.EnumGenerators;

namespace Banking.Simulation.Core.Models.Enums;

[EnumExtensions]
public enum WebhookType
{
    PaymentInitiated,
    CreditRequestInitiated,
    CreditRequestFailed,
    CreditRequestApproved,
    PaymentCompleted,
    PaymentFailed,
}