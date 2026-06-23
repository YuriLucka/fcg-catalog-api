using Catalog.Application.Interfaces;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Interfaces;
using Fcg.Contracts.Events;
using MassTransit;

namespace Catalog.Application.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IGameRepository _games;
    private readonly IUserGameRepository _userGames;
    private readonly IPublishEndpoint _publish;

    public PurchaseService(IGameRepository games, IUserGameRepository userGames, IPublishEndpoint publish)
    {
        _games = games;
        _userGames = userGames;
        _publish = publish;
    }

    public async Task PlaceOrderAsync(Guid userId, Guid gameId, CancellationToken ct = default)
    {
        var game = await _games.GetByIdAsync(gameId)
                   ?? throw new DomainException("Jogo não encontrado.");

        if (await _userGames.ExistsAsync(userId, gameId))
            throw new DomainException("Usuário já possui este jogo.");

        var orderId = Guid.NewGuid();
        await _publish.Publish(new OrderPlacedEvent(orderId, userId, gameId, game.Price), ct);
    }
}
