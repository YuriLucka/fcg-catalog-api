using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces
{
    public interface IGameService
    {
        Task<GameDto> GetByIdAsync(Guid id);
        Task<IEnumerable<GameDto>> GetAllAsync();
        Task<GameDto> CreateAsync(CreateGameDto dto);
        Task<GameDto> UpdateAsync(Guid id, UpdateGameDto dto);
        Task DeleteAsync(Guid id);
        Task<IReadOnlyList<GameDto>> SearchAsync(string query);
        Task EnsureSearchIndexAsync();
    }
}
