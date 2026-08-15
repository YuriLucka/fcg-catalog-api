using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Mongo
{
    [BsonIgnoreExtraElements]
    public class ReviewDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonRepresentation(BsonType.String)]
        public Guid GameId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MongoReviewRepository : IReviewRepository
    {
        private readonly IMongoCollection<ReviewDocument> _collection;

        public MongoReviewRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<ReviewDocument>("reviews");
        }

        public Task AddAsync(Review review)
        {
            var doc = new ReviewDocument
            {
                GameId = review.GameId,
                UserId = review.UserId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
            return _collection.InsertOneAsync(doc);
        }

        public async Task<IEnumerable<Review>> GetByGameIdAsync(Guid gameId)
        {
            var docs = await _collection.Find(d => d.GameId == gameId).ToListAsync();
            return docs.Select(ToEntity);
        }

        private static Review ToEntity(ReviewDocument doc)
        {
            var review = new Review(doc.GameId, doc.UserId, doc.Rating, doc.Comment);
            typeof(Review).GetProperty(nameof(Review.Id))!.SetValue(review, doc.Id);
            typeof(Review).GetProperty(nameof(Review.CreatedAt))!.SetValue(review, doc.CreatedAt);
            return review;
        }
    }
}
