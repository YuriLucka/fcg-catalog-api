using Catalog.Domain.Exceptions;

namespace Catalog.Domain.Entities
{
    public class Game
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // EF Core constructor
        protected Game() { }

        public Game(string title, decimal price, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Título do jogo é obrigatório.");

            if (price < 0)
                throw new DomainException("Preço não pode ser negativo.");

            Id = Guid.NewGuid();
            Title = title;
            Price = price;
            Description = description;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(string title, decimal price, string? description)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Título do jogo é obrigatório.");

            if (price < 0)
                throw new DomainException("Preço não pode ser negativo.");

            Title = title;
            Price = price;
            Description = description;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
