using System.Text.Json;
using Catalog.Application.DTOs;
using Catalog.Application.Exceptions;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services
{
    public class GameService : IGameService
    {
        private const string AllGamesCacheKey = "catalog:games:all";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };

        private readonly IGameRepository _gameRepository;
        private readonly IDistributedCache _cache;
        private readonly IGameSearchService _search;
        private readonly ILogger<GameService> _logger;

        public GameService(IGameRepository gameRepository, IDistributedCache cache, IGameSearchService search, ILogger<GameService> logger)
        {
            _gameRepository = gameRepository;
            _cache = cache;
            _search = search;
            _logger = logger;
        }

        public async Task<GameDto> GetByIdAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new DomainException($"Jogo com Id '{id}' não encontrado.");

            return MapToDto(game);
        }

        public async Task<IEnumerable<GameDto>> GetAllAsync()
        {
            var cached = await TryGetCacheAsync();
            if (cached is not null)
                return JsonSerializer.Deserialize<List<GameDto>>(cached)!;

            var games = await _gameRepository.GetAllAsync();
            var dtos = games.Select(MapToDto).ToList();

            await TrySetCacheAsync(dtos);
            return dtos;
        }

        public async Task<GameDto> CreateAsync(CreateGameDto dto)
        {
            var game = new Game(dto.Title, dto.Price, dto.Description);
            await _gameRepository.AddAsync(game);
            await TryRemoveCacheAsync();
            var created = MapToDto(game);
            await TryIndexAsync(created);
            return created;
        }

        public async Task<GameDto> UpdateAsync(Guid id, UpdateGameDto dto)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new DomainException($"Jogo com Id '{id}' não encontrado.");

            game.Update(dto.Title, dto.Price, dto.Description);
            await _gameRepository.UpdateAsync(game);
            await TryRemoveCacheAsync();

            var updated = MapToDto(game);
            await TryIndexAsync(updated);
            return updated;
        }

        public async Task DeleteAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new DomainException($"Jogo com Id '{id}' não encontrado.");

            await _gameRepository.DeleteAsync(game.Id);
            await TryRemoveCacheAsync();
            await TryRemoveIndexAsync(game.Id);
        }

        public async Task<IReadOnlyList<GameDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new DomainException("Informe o termo de busca.");

            try
            {
                return await _search.SearchAsync(query.Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao consultar o Elasticsearch (termo '{Query}').", query);
                throw new SearchUnavailableException(ex);
            }
        }

        public async Task EnsureSearchIndexAsync()
        {
            try
            {
                var games = await _gameRepository.GetAllAsync();
                await _search.EnsureIndexAsync(games.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao preparar o índice de busca no Elasticsearch. A API segue sem reindexar.");
            }
        }

        private async Task<byte[]?> TryGetCacheAsync()
        {
            try
            {
                return await _cache.GetAsync(AllGamesCacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao ler o cache Redis (chave '{CacheKey}'). Consultando o SQL Server normalmente.", AllGamesCacheKey);
                return null;
            }
        }

        private async Task TrySetCacheAsync(List<GameDto> dtos)
        {
            try
            {
                await _cache.SetAsync(AllGamesCacheKey, JsonSerializer.SerializeToUtf8Bytes(dtos), CacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao gravar o cache Redis (chave '{CacheKey}').", AllGamesCacheKey);
            }
        }

        private async Task TryRemoveCacheAsync()
        {
            try
            {
                await _cache.RemoveAsync(AllGamesCacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao invalidar o cache Redis (chave '{CacheKey}').", AllGamesCacheKey);
            }
        }

        private async Task TryIndexAsync(GameDto game)
        {
            try
            {
                await _search.IndexAsync(game);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao indexar o jogo '{GameId}' no Elasticsearch.", game.Id);
            }
        }

        private async Task TryRemoveIndexAsync(Guid id)
        {
            try
            {
                await _search.RemoveAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao remover o jogo '{GameId}' do Elasticsearch.", id);
            }
        }

        private static GameDto MapToDto(Game game) => new(
            game.Id,
            game.Title,
            game.Description,
            game.Price,
            game.IsActive,
            game.CreatedAt
        );
    }
}
