using Catalog.Application.DTOs;
using Catalog.Application.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;

namespace Catalog.Infrastructure.Search
{
    public class ElasticGameSearchService : IGameSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly string _index;

        public ElasticGameSearchService(ElasticsearchClient client, IOptions<ElasticsearchSettings> settings)
        {
            _client = client;
            _index = settings.Value.IndexName;
        }

        public async Task IndexAsync(GameDto game, CancellationToken ct = default)
        {
            var response = await _client.IndexAsync(game, _index, game.Id, ct);
            EnsureValid(response.IsValidResponse, response.DebugInformation);
        }

        public async Task RemoveAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _client.DeleteAsync(_index, id, ct);
            if (!response.IsValidResponse && response.Result != Result.NotFound)
                throw new InvalidOperationException(response.DebugInformation);
        }

        public async Task<IReadOnlyList<GameDto>> SearchAsync(string query, CancellationToken ct = default)
        {
            var request = new SearchRequest<GameDto>(_index)
            {
                Size = 20,
                Query = new MultiMatchQuery
                {
                    Query = query,
                    Fields = new[] { "title^3", "description" },
                    Fuzziness = new Fuzziness("AUTO")
                }
            };

            var response = await _client.SearchAsync<GameDto>(request, ct);
            EnsureValid(response.IsValidResponse, response.DebugInformation);
            return response.Documents.ToList();
        }

        public async Task EnsureIndexAsync(IEnumerable<GameDto> games, CancellationToken ct = default)
        {
            var exists = await _client.Indices.ExistsAsync(_index, ct);
            if (!exists.Exists)
            {
                var create = await _client.Indices.CreateAsync(_index, ct);
                EnsureValid(create.IsValidResponse, create.DebugInformation);
            }

            // Sempre reenvia todos os jogos (upsert por _id): um boot que falhou no meio não deixa o índice vazio.

            var docs = games.ToList();
            if (docs.Count == 0)
                return;

            var bulk = await _client.BulkAsync(b => b
                .Index(_index)
                .IndexMany(docs, (op, doc) => op.Id(doc.Id)), ct);
            EnsureValid(bulk.IsValidResponse && !bulk.Errors, bulk.DebugInformation);
        }

        private static void EnsureValid(bool valid, string debugInformation)
        {
            if (!valid)
                throw new InvalidOperationException(debugInformation);
        }
    }
}
