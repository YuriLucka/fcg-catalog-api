namespace Catalog.Application.Interfaces;

public interface IPurchaseService
{
    Task PlaceOrderAsync(Guid userId, Guid gameId, CancellationToken ct = default);
}
