using Catalog.Application.DTOs;
using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace Catalog.Tests.Application
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _gameRepoMock = new Mock<IGameRepository>();
            _cacheMock = new Mock<IDistributedCache>();
            _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default)).ReturnsAsync((byte[]?)null);
            _gameService = new GameService(_gameRepoMock.Object, _cacheMock.Object);
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
            var service = new GameService(repoMock.Object, cacheMock.Object);

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
            var service = new GameService(repoMock.Object, cacheMock.Object);

            await service.CreateAsync(new CreateGameDto("Novo Jogo", 10m, null));

            cacheMock.Verify(c => c.RemoveAsync("catalog:games:all", default), Times.Once);
        }
    }
}
