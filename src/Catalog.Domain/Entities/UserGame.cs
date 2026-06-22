namespace Catalog.Domain.Entities
{
    public class UserGame
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid GameId { get; private set; }

        private UserGame() { }   // EF

        public UserGame(Guid userId, Guid gameId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            GameId = gameId;
        }
    }
}
