using System.Text.Json;
using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Catalog.Domain.Entities;
using Catalog.Domain.Exceptions;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Catalog.Application.Services
{
    public class GameService : IGameService
    {
        private const string AllGamesCacheKey = "catalog:games:all";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        };

        private readonly IGameRepository _gameRepository;
        private readonly IDistributedCache _cache;

        public GameService(IGameRepository gameRepository, IDistributedCache cache)
        {
            _gameRepository = gameRepository;
            _cache = cache;
        }

        public async Task<GameDto> GetByIdAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new DomainException($"Jogo com Id '{id}' não encontrado.");

            return MapToDto(game);
        }

        public async Task<IEnumerable<GameDto>> GetAllAsync()
        {
            var cached = await _cache.GetAsync(AllGamesCacheKey);
            if (cached is not null)
                return JsonSerializer.Deserialize<List<GameDto>>(cached)!;

            var games = await _gameRepository.GetAllAsync();
            var dtos = games.Select(MapToDto).ToList();

            await _cache.SetAsync(AllGamesCacheKey, JsonSerializer.SerializeToUtf8Bytes(dtos), CacheOptions);
            return dtos;
        }

        public async Task<GameDto> CreateAsync(CreateGameDto dto)
        {
            var game = new Game(dto.Title, dto.Price, dto.Description);
            await _gameRepository.AddAsync(game);
            await _cache.RemoveAsync(AllGamesCacheKey);
            return MapToDto(game);
        }

        public async Task<GameDto> UpdateAsync(Guid id, UpdateGameDto dto)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new DomainException($"Jogo com Id '{id}' não encontrado.");

            game.Update(dto.Title, dto.Price, dto.Description);
            await _gameRepository.UpdateAsync(game);
            await _cache.RemoveAsync(AllGamesCacheKey);

            return MapToDto(game);
        }

        public async Task DeleteAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id)
                ?? throw new DomainException($"Jogo com Id '{id}' não encontrado.");

            await _gameRepository.DeleteAsync(game.Id);
            await _cache.RemoveAsync(AllGamesCacheKey);
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
