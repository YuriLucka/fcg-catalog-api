using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain
{
    public class ReviewEntityTests
    {
        [Fact]
        public void Construtor_DeveCriarReview_QuandoDadosValidos()
        {
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var review = new Review(gameId, userId, 5, "Excelente jogo");

            Assert.Equal(gameId, review.GameId);
            Assert.Equal(userId, review.UserId);
            Assert.Equal(5, review.Rating);
            Assert.Equal("Excelente jogo", review.Comment);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public void Construtor_DeveLancarExcecao_QuandoRatingForaDoIntervalo(int rating)
        {
            Assert.Throws<DomainException>(() =>
                new Review(Guid.NewGuid(), Guid.NewGuid(), rating, "comentário"));
        }
    }
}
