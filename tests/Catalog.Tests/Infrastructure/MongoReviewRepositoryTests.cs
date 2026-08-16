using Catalog.Infrastructure.Mongo;
using Moq;
using MongoDB.Driver;

namespace Catalog.Tests.Infrastructure
{
    public class MongoReviewRepositoryTests
    {
        [Fact]
        public void Construtor_DeveCriarIndice_NoCampoGameId()
        {
            var indexManagerMock = new Mock<IMongoIndexManager<ReviewDocument>>();
            var collectionMock = new Mock<IMongoCollection<ReviewDocument>>();
            collectionMock.Setup(c => c.Indexes).Returns(indexManagerMock.Object);
            var databaseMock = new Mock<IMongoDatabase>();
            databaseMock
                .Setup(d => d.GetCollection<ReviewDocument>("reviews", null))
                .Returns(collectionMock.Object);

            _ = new MongoReviewRepository(databaseMock.Object);

            indexManagerMock.Verify(m => m.CreateOneAsync(
                It.IsAny<CreateIndexModel<ReviewDocument>>(),
                It.IsAny<CreateOneIndexOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
