using Catalog.Domain.Entities;

namespace Catalog.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task<IEnumerable<Review>> GetByGameIdAsync(Guid gameId);
        Task<bool> ExistsAsync(Guid userId, Guid gameId);
    }
}
