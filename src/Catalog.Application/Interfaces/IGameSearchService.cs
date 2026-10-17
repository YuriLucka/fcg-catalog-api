using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces
{
    public interface IGameSearchService
    {
        Task IndexAsync(GameDto game, CancellationToken ct = default);
        Task RemoveAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<GameDto>> SearchAsync(string query, CancellationToken ct = default);
        Task EnsureIndexAsync(IEnumerable<GameDto> games, CancellationToken ct = default);
    }
}
