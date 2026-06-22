using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;

namespace Catalog.Tests.Domain
{
    public class GameEntityTests
    {
        [Fact]
        public void Game_DeveSerCriado_QuandoDadosValidos()
        {
            var game = new Game("Counter Strike", 59.90m, "FPS clássico");

            Assert.Equal("Counter Strike", game.Title);
            Assert.Equal(59.90m, game.Price);
            Assert.True(game.IsActive);
            Assert.NotEqual(Guid.Empty, game.Id);
        }

        [Fact]
        public void Game_DeveLancarExcecao_QuandoTituloVazio()
        {
            var ex = Assert.Throws<DomainException>(() =>
                new Game("", 59.90m));

            Assert.Equal("Título do jogo é obrigatório.", ex.Message);
        }

        [Fact]
        public void Game_DeveLancarExcecao_QuandoPrecoNegativo()
        {
            var ex = Assert.Throws<DomainException>(() =>
                new Game("Jogo", -10m));

            Assert.Equal("Preço não pode ser negativo.", ex.Message);
        }

        [Fact]
        public void Game_DevePermitirPrecoZero()
        {
            var exception = Record.Exception(() => new Game("Jogo Grátis", 0m));

            Assert.Null(exception);
        }

        [Fact]
        public void Game_DeveSerDesativado_QuandoChamadoDeactivate()
        {
            var game = new Game("Jogo", 10m);

            game.Deactivate();

            Assert.False(game.IsActive);
        }

        [Fact]
        public void Game_DeveSerAtivado_QuandoChamadoActivate()
        {
            var game = new Game("Jogo", 10m);
            game.Deactivate();

            game.Activate();

            Assert.True(game.IsActive);
        }

        [Fact]
        public void Game_DeveAtualizarDados_QuandoChamadoUpdate()
        {
            var game = new Game("Titulo Antigo", 10m);

            game.Update("Titulo Novo", 99m, "Nova descrição");

            Assert.Equal("Titulo Novo", game.Title);
            Assert.Equal(99m, game.Price);
            Assert.Equal("Nova descrição", game.Description);
        }
    }
}
