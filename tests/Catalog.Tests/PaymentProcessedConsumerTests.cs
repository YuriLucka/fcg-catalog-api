using Catalog.Application.Consumers;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Fcg.Contracts.Events;
using MassTransit;
using Moq;
using Xunit;

namespace Catalog.Tests.Application;

public class PaymentProcessedConsumerTests
{
    // Fake repo backed by a list — exercises real idempotency logic
    private sealed class FakeUserGameRepo : IUserGameRepository
    {
        public readonly List<UserGame> Items = new();
        public Task<bool> ExistsAsync(Guid userId, Guid gameId) =>
            Task.FromResult(Items.Any(x => x.UserId == userId && x.GameId == gameId));
        public Task AddAsync(UserGame ug) { Items.Add(ug); return Task.CompletedTask; }
        public Task<IEnumerable<UserGame>> GetByUserAsync(Guid userId) =>
            Task.FromResult(Items.Where(x => x.UserId == userId).AsEnumerable());
    }

    private static ConsumeContext<PaymentProcessedEvent> Ctx(PaymentProcessedEvent e)
    {
        var ctx = new Mock<ConsumeContext<PaymentProcessedEvent>>();
        ctx.SetupGet(c => c.Message).Returns(e);
        ctx.SetupGet(c => c.CancellationToken).Returns(CancellationToken.None);
        return ctx.Object;
    }

    [Fact]
    public async Task Approved_adds_game_once_and_is_idempotent()
    {
        var repo = new FakeUserGameRepo();
        var sut = new PaymentProcessedConsumer(repo);
        var userId = Guid.NewGuid(); var gameId = Guid.NewGuid();
        var evt = new PaymentProcessedEvent(Guid.NewGuid(), userId, gameId, PaymentStatus.Approved);

        await sut.Consume(Ctx(evt));
        await sut.Consume(Ctx(evt)); // duplicado

        Assert.Single(repo.Items);
        Assert.Equal(userId, repo.Items[0].UserId);
        Assert.Equal(gameId, repo.Items[0].GameId);
    }

    [Fact]
    public async Task Rejected_does_not_add()
    {
        var repo = new FakeUserGameRepo();
        var sut = new PaymentProcessedConsumer(repo);
        var evt = new PaymentProcessedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), PaymentStatus.Rejected);

        await sut.Consume(Ctx(evt));

        Assert.Empty(repo.Items);
    }
}
