using Catalog.Application.Interfaces;
using Catalog.Domain.Interfaces;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Mongo;
using Catalog.Infrastructure.Repositories;
using Catalog.Infrastructure.Search;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
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

            services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));
            services.AddSingleton(sp =>
            {
                var es = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
                    ?? throw new InvalidOperationException("Seção 'Elasticsearch' não configurada.");
                if (string.IsNullOrWhiteSpace(es.Url))
                    throw new InvalidOperationException("Elasticsearch:Url não configurado.");

                var clientSettings = new ElasticsearchClientSettings(new Uri(es.Url))
                    .RequestTimeout(TimeSpan.FromSeconds(10));
                if (!string.IsNullOrWhiteSpace(es.ApiKey))
                    clientSettings = clientSettings.Authentication(new ApiKey(es.ApiKey));

                return new ElasticsearchClient(clientSettings);
            });
            services.AddScoped<IGameSearchService, ElasticGameSearchService>();

            return services;
        }
    }
}
