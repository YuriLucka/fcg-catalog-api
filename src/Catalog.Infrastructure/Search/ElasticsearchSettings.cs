namespace Catalog.Infrastructure.Search
{
    public class ElasticsearchSettings
    {
        public string Url { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
        public string IndexName { get; set; } = "games";
    }
}
