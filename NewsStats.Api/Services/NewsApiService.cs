using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Distributed;
using NewsStats.Api.Models;

namespace NewsStats.Api.Services;

public class NewsApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IDistributedCache _cache;
    private readonly ILogger<NewsApiService> _logger;

    public NewsApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IDistributedCache cache, ILogger<NewsApiService> logger)
    {
        if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
        _httpClient = httpClientFactory.CreateClient("NewsApi");
        _apiKey = configuration["NewsApiKey"] ?? throw new InvalidOperationException("NewsApiKey not configured.");
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<NewsApiResponse?> GetArticlesAsync(string? query, DateTime? from, DateTime? to)
    {
        if (!string.IsNullOrWhiteSpace(query) && !Regex.IsMatch(query, @"^[a-zA-Z0-9\s]+$"))
        {
            return null;
        }

        // Generate cache key by hashing all parameters
        var cacheKeyInput = $"{query ?? ""}|{from:yyyy-MM-dd}|{to:yyyy-MM-dd}";
        var cacheKey = $"articles:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cacheKeyInput)))}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonSerializer.Deserialize<NewsApiResponse>(cachedData);
        }

        var searchQuery = string.IsNullOrWhiteSpace(query) ? "*" : query;
        var endpoint = $"everything?q={Uri.EscapeDataString(searchQuery)}&apiKey={_apiKey}";
        if (from.HasValue) endpoint += $"&from={from.Value:yyyy-MM-dd}";
        if (to.HasValue) endpoint += $"&to={to.Value:yyyy-MM-dd}";

        try
        {
            var newsApiResponse = await _httpClient.GetFromJsonAsync<NewsApiResponse>(endpoint);
            if (newsApiResponse != null)
            {
                // Cache the response for 5 minutes
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(newsApiResponse), cacheOptions);
            }
            return newsApiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching articles from News API");
            throw;
        }
    }
}
