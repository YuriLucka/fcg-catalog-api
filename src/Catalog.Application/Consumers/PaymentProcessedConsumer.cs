using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Fcg.Contracts.Events;
using MassTransit;

namespace Catalog.Application.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IUserGameRepository _library;
    public PaymentProcessedConsumer(IUserGameRepository library) => _library = library;

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> ctx)
    {
        var m = ctx.Message;
        if (m.Status != PaymentStatus.Approved) return;
        if (await _library.ExistsAsync(m.UserId, m.GameId)) return; // idempotência
        await _library.AddAsync(new UserGame(m.UserId, m.GameId));
    }
}
