using Catalog.Application.DTOs;

namespace Catalog.Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateAsync(CreateReviewDto dto, Guid userId);
        Task<IEnumerable<ReviewDto>> GetByGameIdAsync(Guid gameId);
    }
}
