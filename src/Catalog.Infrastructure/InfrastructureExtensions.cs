using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Mongo;
using Catalog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Catalog.Infrastructure
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                ));

            // Repositórios
            services.AddScoped<IGameRepository, GameRepository>();
            services.AddScoped<IUserGameRepository, UserGameRepository>();

            services.Configure<MongoSettings>(configuration.GetSection("MongoSettings"));
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = configuration.GetSection("MongoSettings").Get<MongoSettings>()!;
                return new MongoClient(settings.ConnectionString);
            });
            services.AddScoped(sp =>
            {
                var settings = configuration.GetSection("MongoSettings").Get<MongoSettings>()!;
                return sp.GetRequiredService<IMongoClient>().GetDatabase(settings.Database);
            });
            services.AddScoped<IReviewRepository, MongoReviewRepository>();

            return services;
        }
    }
}
