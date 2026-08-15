using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities
{
    public class Review
    {
        public string Id { get; private set; } = string.Empty;
        public Guid GameId { get; private set; }
        public Guid UserId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Review() { }

        public Review(Guid gameId, Guid userId, int rating, string? comment = null)
        {
            if (rating is < 1 or > 5)
                throw new DomainException("Rating deve estar entre 1 e 5.");

            GameId = gameId;
            UserId = userId;
            Rating = rating;
            Comment = comment;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
