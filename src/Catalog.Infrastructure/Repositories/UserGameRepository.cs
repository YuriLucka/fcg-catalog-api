using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Repositories
{
    public class UserGameRepository : IUserGameRepository
    {
        private readonly AppDbContext _db;

        public UserGameRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid gameId)
        {
            return await _db.UserGames.AnyAsync(x => x.UserId == userId && x.GameId == gameId);
        }

        public async Task AddAsync(UserGame userGame)
        {
            await _db.UserGames.AddAsync(userGame);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserGame>> GetByUserAsync(Guid userId)
        {
            return await _db.UserGames.Where(x => x.UserId == userId).ToListAsync();
        }
    }
}
