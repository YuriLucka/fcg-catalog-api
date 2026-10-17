using Catalog.Application.DTOs;
using Catalog.Application.Exceptions;
using Catalog.Application.Interfaces;
using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Catalog.Tests.Application
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IGameSearchService> _searchMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _gameRepoMock = new Mock<IGameRepository>();
            _cacheMock = new Mock<IDistributedCache>();
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[]?)null);
            _searchMock = new Mock<IGameSearchService>();
            _gameService = new GameService(_gameRepoMock.Object, _cacheMock.Object, _searchMock.Object, NullLogger<GameService>.Instance);
        }

        // ── CreateAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_DeveCriarJogo_QuandoDadosValidos()
        {
            var dto = new CreateGameDto("Counter Strike", 59.90m, "FPS");
            _gameRepoMock.Setup(r => r.AddAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);

            var result = await _gameService.CreateAsync(dto);

            Assert.Equal("Counter Strike", result.Title);
            Assert.Equal(59.90m, result.Price);
            Assert.True(result.IsActive);

            _gameRepoMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DeveLancarExcecao_QuandoTituloVazio()
        {
            var dto = new CreateGameDto("", 59.90m, null);

            await Assert.ThrowsAsync<DomainException>(() =>
                _gameService.CreateAsync(dto));

            _gameRepoMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_DeveLancarExcecao_QuandoPrecoNegativo()
        {
            var dto = new CreateGameDto("Jogo", -1m, null);

            await Assert.ThrowsAsync<DomainException>(() =>
                _gameService.CreateAsync(dto));
        }

        // ── GetById ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_DeveRetornarJogo_QuandoExistir()
        {
            var game = new Game("CS2", 59.90m);
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

            var result = await _gameService.GetByIdAsync(game.Id);

            Assert.Equal(game.Id, result.Id);
            Assert.Equal("CS2", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_DeveLancarExcecao_QuandoNaoExistir()
        {
            _gameRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);

            await Assert.ThrowsAsync<DomainException>(() =>
                _gameService.GetByIdAsync(Guid.NewGuid()));
        }

        // ── UpdateAsync ─────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_DeveAtualizar_QuandoJogoExistir()
        {
            var game = new Game("Titulo Antigo", 10m);
            var dto = new UpdateGameDto("Titulo Novo", 99m, "Desc");

            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
            _gameRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);

            var result = await _gameService.UpdateAsync(game.Id, dto);

            Assert.Equal("Titulo Novo", result.Title);
            Assert.Equal(99m, result.Price);
        }

        // ── Cache ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_DeveConsultarRepositorio_QuandoCacheVazio()
        {
            var cacheMock = new Mock<IDistributedCache>();
            cacheMock.Setup(c => c.GetAsync("catalog:games:all", default)).ReturnsAsync((byte[]?)null);
            var repoMock = new Mock<IGameRepository>();
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Game> { new("CS2", 59.90m) });
            var service = new GameService(repoMock.Object, cacheMock.Object, Mock.Of<IGameSearchService>(), NullLogger<GameService>.Instance);

            var result = await service.GetAllAsync();

            Assert.Single(result);
            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            cacheMock.Verify(c => c.SetAsync("catalog:games:all", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DeveInvalidarCache_QuandoJogoCriado()
        {
            var cacheMock = new Mock<IDistributedCache>();
            var repoMock = new Mock<IGameRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
            var service = new GameService(repoMock.Object, cacheMock.Object, Mock.Of<IGameSearchService>(), NullLogger<GameService>.Instance);

            await service.CreateAsync(new CreateGameDto("Novo Jogo", 10m, null));

            cacheMock.Verify(c => c.RemoveAsync("catalog:games:all", default), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_DeveConsultarRepositorio_QuandoCacheFalhar()
        {
            var cacheMock = new Mock<IDistributedCache>();
            cacheMock.Setup(c => c.GetAsync("catalog:games:all", default))
                .ThrowsAsync(new Exception("Redis indisponível"));
            var repoMock = new Mock<IGameRepository>();
            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Game> { new("CS2", 59.90m) });
            var service = new GameService(repoMock.Object, cacheMock.Object, Mock.Of<IGameSearchService>(), NullLogger<GameService>.Instance);

            var result = await service.GetAllAsync();

            Assert.Single(result);
            repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_DeveCompletarComSucesso_QuandoInvalidacaoDeCacheFalhar()
        {
            var cacheMock = new Mock<IDistributedCache>();
            cacheMock.Setup(c => c.RemoveAsync("catalog:games:all", default))
                .ThrowsAsync(new Exception("Redis indisponível"));
            var repoMock = new Mock<IGameRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<Game>())).Returns(Task.CompletedTask);
            var service = new GameService(repoMock.Object, cacheMock.Object, Mock.Of<IGameSearchService>(), NullLogger<GameService>.Instance);

            var result = await service.CreateAsync(new CreateGameDto("Novo Jogo", 10m, null));

            Assert.Equal("Novo Jogo", result.Title);
            repoMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
        }
            // ── Busca (Elasticsearch) ───────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_DeveIndexarJogo()
        {
            var result = await _gameService.CreateAsync(new CreateGameDto("Zelda", 199m, "Aventura"));

            _searchMock.Verify(s => s.IndexAsync(It.Is<GameDto>(g => g.Id == result.Id && g.Title == "Zelda"), default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_DeveReindexarJogo()
        {
            var game = new Game("Zelda", 199m);
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

            await _gameService.UpdateAsync(game.Id, new UpdateGameDto("Zelda TOTK", 249m, null));

            _searchMock.Verify(s => s.IndexAsync(It.Is<GameDto>(g => g.Id == game.Id && g.Title == "Zelda TOTK"), default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeveRemoverDoIndice()
        {
            var game = new Game("Zelda", 199m);
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

            await _gameService.DeleteAsync(game.Id);

            _searchMock.Verify(s => s.RemoveAsync(game.Id, default), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_NaoFalha_QuandoIndexacaoFalha()
        {
            _searchMock.Setup(s => s.IndexAsync(It.IsAny<GameDto>(), default)).ThrowsAsync(new HttpRequestException("es down"));

            var result = await _gameService.CreateAsync(new CreateGameDto("Zelda", 199m, null));

            Assert.Equal("Zelda", result.Title);
            _gameRepoMock.Verify(r => r.AddAsync(It.IsAny<Game>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NaoFalha_QuandoRemocaoDoIndiceFalha()
        {
            var game = new Game("Zelda", 199m);
            _gameRepoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
            _searchMock.Setup(s => s.RemoveAsync(game.Id, default)).ThrowsAsync(new HttpRequestException("es down"));

            await _gameService.DeleteAsync(game.Id);

            _gameRepoMock.Verify(r => r.DeleteAsync(game.Id), Times.Once);
        }

        [Fact]
        public async Task SearchAsync_DeveRetornarResultadosDoIndice()
        {
            var hit = new GameDto(Guid.NewGuid(), "Zelda", null, 199m, true, DateTime.UtcNow);
            _searchMock.Setup(s => s.SearchAsync("zelad", default)).ReturnsAsync(new List<GameDto> { hit });

            var result = await _gameService.SearchAsync("  zelad ");

            Assert.Single(result);
            Assert.Equal("Zelda", result[0].Title);
            _gameRepoMock.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SearchAsync_DeveLancarDomainException_QuandoTermoVazio(string query)
        {
            await Assert.ThrowsAsync<DomainException>(() => _gameService.SearchAsync(query));
            _searchMock.Verify(s => s.SearchAsync(It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task SearchAsync_DeveLancarSearchUnavailable_QuandoIndiceIndisponivel()
        {
            _searchMock.Setup(s => s.SearchAsync(It.IsAny<string>(), default)).ThrowsAsync(new HttpRequestException("es down"));

            await Assert.ThrowsAsync<SearchUnavailableException>(() => _gameService.SearchAsync("zelda"));
        }

        [Fact]
        public async Task EnsureSearchIndexAsync_DeveEnviarTodosOsJogos()
        {
            var games = new List<Game> { new("Zelda", 199m), new("Mario", 99m) };
            _gameRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

            await _gameService.EnsureSearchIndexAsync();

            _searchMock.Verify(s => s.EnsureIndexAsync(It.Is<IEnumerable<GameDto>>(l => l.Count() == 2), default), Times.Once);
        }

        [Fact]
        public async Task EnsureSearchIndexAsync_NaoFalha_QuandoIndiceIndisponivel()
        {
            _gameRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Game>());
            _searchMock.Setup(s => s.EnsureIndexAsync(It.IsAny<IEnumerable<GameDto>>(), default)).ThrowsAsync(new HttpRequestException("es down"));

            await _gameService.EnsureSearchIndexAsync();
        }
    }
}
