using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IGameRepository _gameRepository;

        public ReviewService(IReviewRepository reviewRepository, IGameRepository gameRepository)
        {
            _reviewRepository = reviewRepository;
            _gameRepository = gameRepository;
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto, Guid userId)
        {
            _ = await _gameRepository.GetByIdAsync(dto.GameId)
                ?? throw new DomainException("Jogo não encontrado.");

            if (await _reviewRepository.ExistsAsync(userId, dto.GameId))
                throw new DomainException("Você já avaliou este jogo.");

            var review = new Review(dto.GameId, userId, dto.Rating, dto.Comment);
            await _reviewRepository.AddAsync(review);
            return MapToDto(review);
        }

        public async Task<IEnumerable<ReviewDto>> GetByGameIdAsync(Guid gameId)
        {
            var reviews = await _reviewRepository.GetByGameIdAsync(gameId);
            return reviews.Select(MapToDto);
        }

        private static ReviewDto MapToDto(Review r) =>
            new(r.Id, r.GameId, r.UserId, r.Rating, r.Comment, r.CreatedAt);
    }
}
