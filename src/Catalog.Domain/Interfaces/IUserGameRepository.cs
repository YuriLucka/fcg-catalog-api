using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces
{
    public interface IUserGameRepository
    {
        Task<bool> ExistsAsync(Guid userId, Guid gameId);
        Task AddAsync(UserGame userGame);
        Task<IEnumerable<UserGame>> GetByUserAsync(Guid userId);
    }
}
