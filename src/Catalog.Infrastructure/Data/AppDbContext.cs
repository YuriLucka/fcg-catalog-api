using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Game> Games => Set<Game>();
        public DbSet<UserGame> UserGames => Set<UserGame>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica todos os mappings do assembly automaticamente
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public static async Task SeedAsync(AppDbContext context)
        {
            await SeedGamesAsync(context);
        }

        private static async Task SeedGamesAsync(AppDbContext context)
        {
            if (await context.Games.AnyAsync())
                return;

            var games = new List<Game>
            {
                new("Minecraft",            89.9m,  "Sandbox de construção e sobrevivência."),
                new("Hollow Knight",        29.9m,  "Metroidvania de exploração e combate."),
                new("Stardew Valley",       34.9m,  "RPG de fazenda e vida no campo."),
                new("Celeste",              19.9m,  "Plataforma com desafios de precisão."),
                new("The Witcher 3",        79.9m,  "RPG de mundo aberto com história épica."),
                new("Red Dead Redemption 2",99.9m,  "Aventura no velho oeste americano."),
                new("Cyberpunk 2077",       89.9m,  "RPG futurista em cidade distópica."),
                new("Hades",                37.99m, "Roguelike de ação com narrativa progressiva."),
                new("Elden Ring",           199.9m, "Action RPG desafiador em mundo aberto."),
                new("God of War",           99.9m,  "Aventura mitológica com Kratos e Atreus."),
                new("Sekiro: Shadows Die Twice", 159.9m, "Action RPG samurai com combate exigente."),
                new("Death Stranding",      79.9m,  "Jogo de entrega em mundo pós-apocalíptico."),
                new("Half-Life: Alyx",      89.9m,  "FPS de realidade virtual da Valve."),
                new("Counter-Strike 2",     49.9m,  "FPS tático competitivo multiplayer."),
                new("Portal 2",             29.9m,  "Puzzle em primeira pessoa com portais e física."),
                new("Cuphead",              39.9m,  "Run and gun com estética dos anos 1930."),
                new("Ori and the Will of the Wisps", 49.9m, "Plataforma de aventura com visual artístico."),
                new("It Takes Two",         99.9m,  "Co-op obrigatório com mecânicas criativas."),
                new("Disco Elysium",        59.9m,  "RPG investigativo focado em narrativa e escolhas."),
                new("Tunic",                49.9m,  "Aventura com raposa explorando ruínas antigas."),
            };

            await context.Games.AddRangeAsync(games);
            await context.SaveChangesAsync();
        }
    }
}
