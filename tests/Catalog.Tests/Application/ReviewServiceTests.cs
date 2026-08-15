using Catalog.Application.DTOs;
using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Moq;

namespace Catalog.Tests.Application
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _repoMock;
        private readonly ReviewService _service;

        public ReviewServiceTests()
        {
            _repoMock = new Mock<IReviewRepository>();
            _service = new ReviewService(_repoMock.Object);
        }

        [Fact]
        public async Task CreateAsync_DeveSalvarReview_QuandoDadosValidos()
        {
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var dto = new CreateReviewDto(gameId, 4, "Muito bom");

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Review>())).Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto, userId);

            Assert.Equal(gameId, result.GameId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(4, result.Rating);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
        }

        [Fact]
        public async Task GetByGameIdAsync_DeveRetornarLista()
        {
            var gameId = Guid.NewGuid();
            var reviews = new List<Review> { new(gameId, Guid.NewGuid(), 5, "Top") };
            _repoMock.Setup(r => r.GetByGameIdAsync(gameId)).ReturnsAsync(reviews);

            var result = await _service.GetByGameIdAsync(gameId);

            Assert.Single(result);
        }
    }
}
