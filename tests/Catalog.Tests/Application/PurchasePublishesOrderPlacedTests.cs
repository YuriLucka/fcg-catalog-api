using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Fcg.Contracts.Events;
using MassTransit;
using Moq;
using Xunit;

namespace Catalog.Tests.Application;

public class PurchasePublishesOrderPlacedTests
{
    [Fact]
    public async Task PlaceOrder_publishes_OrderPlacedEvent_with_game_price()
    {
        var userId = Guid.NewGuid();
        var game = new Game("Doom", 59.90m);
        var games = new Mock<IGameRepository>();
        games.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        var publish = new Mock<IPublishEndpoint>();
        var sut = new PurchaseService(games.Object, publish.Object);

        await sut.PlaceOrderAsync(userId, game.Id);

        publish.Verify(p => p.Publish(
            It.Is<OrderPlacedEvent>(e => e.UserId == userId && e.GameId == game.Id && e.Price == 59.90m),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlaceOrder_throws_when_game_not_found()
    {
        var games = new Mock<IGameRepository>();
        games.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);
        var publish = new Mock<IPublishEndpoint>();
        var sut = new PurchaseService(games.Object, publish.Object);

        await Assert.ThrowsAsync<Catalog.Domain.Exceptions.DomainException>(
            () => sut.PlaceOrderAsync(Guid.NewGuid(), Guid.NewGuid()));
        publish.Verify(p => p.Publish(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
