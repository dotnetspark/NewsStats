using System.Text.Json;
using NewStats.Web.Models;
using StackExchange.Redis;

namespace NewStats.Web.Services;

public class RedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(15);

    private const string SearchResultsKeyPrefix = "search:";
    private const string LastSearchKey = "last_search";
    private const string ArticlesKeyPrefix = "article:";

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<SearchResult?> GetSearchResultsAsync(string query)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{SearchResultsKeyPrefix}{query.ToLowerInvariant()}";
            var cached = await db.StringGetAsync(key);

            if (cached.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<SearchResult>(cached.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search results from cache for query: {Query}", query);
            return null;
        }
    }

    public async Task CacheSearchResultsAsync(string query, List<Article> articles)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{SearchResultsKeyPrefix}{query.ToLowerInvariant()}";
            var searchResult = new SearchResult
            {
                Query = query,
                Articles = articles,
                CachedAt = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(searchResult);
            await db.StringSetAsync(key, json, _defaultExpiry);

            // Also cache individual articles for quick lookup
            foreach (var article in articles)
            {
                await CacheArticleAsync(article);
            }

            _logger.LogInformation("Cached search results for query: {Query} with {Count} articles", query, articles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching search results for query: {Query}", query);
        }
    }

    public async Task<Article?> GetArticleAsync(string articleId)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{ArticlesKeyPrefix}{articleId}";
            var cached = await db.StringGetAsync(key);

            if (cached.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<Article>(cached.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting article from cache: {ArticleId}", articleId);
            return null;
        }
    }

    public async Task CacheArticleAsync(Article article)
    {
        try
        {
            var db = _redis.GetDatabase();
            var key = $"{ArticlesKeyPrefix}{article.Id}";
            var json = JsonSerializer.Serialize(article);
            await db.StringSetAsync(key, json, _defaultExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching article: {ArticleId}", article.Id);
        }
    }

    public async Task<string?> GetLastSearchQueryAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            var cached = await db.StringGetAsync(LastSearchKey);
            return cached.IsNullOrEmpty ? null : cached.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last search query from cache");
            return null;
        }
    }

    public async Task SetLastSearchQueryAsync(string query)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(LastSearchKey, query, _defaultExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting last search query in cache");
        }
    }
}
