namespace Fcg.Contracts.Events;

public record PaymentProcessedEvent(Guid OrderId, Guid UserId, Guid GameId, PaymentStatus Status);
