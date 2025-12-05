using System.Net.Http.Headers;
using System.Text.Json;
using NewStats.Web.Models;

namespace NewStats.Web.Services;

public class NewsApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsApiService> _logger;

    public NewsApiService(HttpClient httpClient, IConfiguration configuration, ILogger<NewsApiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<Article>> SearchArticlesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        try
        {
            var apiKey = _configuration["NewsApi:ApiKey"] ?? "";
            var baseUrl = _configuration["NewsApi:BaseUrl"] ?? "https://newsapi.org/v2";

            var requestUrl = $"{baseUrl}/everything?q={Uri.EscapeDataString(query)}&sortBy=publishedAt&pageSize=20";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("X-Api-Key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("NewsAPI returned status code {StatusCode}", response.StatusCode);
                return GetMockArticles(query);
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var newsResponse = JsonSerializer.Deserialize<NewsApiResponse>(content, options);

            if (newsResponse?.Articles == null || newsResponse.Articles.Count == 0)
            {
                _logger.LogInformation("No articles found for query: {Query}", query);
                return GetMockArticles(query);
            }

            return newsResponse.Articles
                .Where(a => !string.IsNullOrEmpty(a.Title) && a.Title != "[Removed]")
                .Select(a => new Article
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = a.Title ?? "Untitled",
                    Description = a.Description ?? "No description available",
                    Url = a.Url ?? "#",
                    UrlToImage = a.UrlToImage ?? "",
                    PublishedAt = a.PublishedAt,
                    Author = a.Author ?? "Unknown",
                    Source = a.Source?.Name ?? "Unknown Source",
                    Content = a.Content ?? a.Description ?? "No content available"
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching articles from NewsAPI for query: {Query}", query);
            return GetMockArticles(query);
        }
    }

    private static List<Article> GetMockArticles(string query)
    {
        return
        [
            new Article
            {
                Id = "1",
                Title = $"Breaking: Major developments in {query}",
                Description = $"Latest updates and comprehensive coverage of {query} related news and events happening around the world.",
                Url = "https://example.com/article1",
                UrlToImage = "https://via.placeholder.com/400x200?text=News+Article",
                PublishedAt = DateTime.UtcNow.AddHours(-2),
                Author = "John Smith",
                Source = "Tech News Daily",
                Content = $"This is a detailed article about {query}. The article covers various aspects and provides in-depth analysis of the topic."
            },
            new Article
            {
                Id = "2",
                Title = $"Analysis: What {query} means for the industry",
                Description = $"Expert analysis on how {query} is shaping the future of technology and business.",
                Url = "https://example.com/article2",
                UrlToImage = "https://via.placeholder.com/400x200?text=Analysis",
                PublishedAt = DateTime.UtcNow.AddHours(-5),
                Author = "Jane Doe",
                Source = "Business Insider",
                Content = $"Industry experts weigh in on {query} and its potential impact on global markets."
            },
            new Article
            {
                Id = "3",
                Title = $"How {query} is changing everything",
                Description = $"A deep dive into the transformative effects of {query} on our daily lives.",
                Url = "https://example.com/article3",
                UrlToImage = "https://via.placeholder.com/400x200?text=Deep+Dive",
                PublishedAt = DateTime.UtcNow.AddHours(-8),
                Author = "Mike Johnson",
                Source = "The Guardian",
                Content = $"From households to corporations, {query} continues to revolutionize how we live and work."
            },
            new Article
            {
                Id = "4",
                Title = $"The future of {query}: Predictions for 2025",
                Description = $"What can we expect from {query} in the coming year? Experts share their forecasts.",
                Url = "https://example.com/article4",
                UrlToImage = "https://via.placeholder.com/400x200?text=Future+Trends",
                PublishedAt = DateTime.UtcNow.AddHours(-12),
                Author = "Sarah Wilson",
                Source = "Future Tech Weekly",
                Content = $"As we look ahead, {query} is poised to become even more influential in shaping technology trends."
            },
            new Article
            {
                Id = "5",
                Title = $"Beginner's guide to understanding {query}",
                Description = $"New to {query}? This comprehensive guide will help you get started.",
                Url = "https://example.com/article5",
                UrlToImage = "https://via.placeholder.com/400x200?text=Beginners+Guide",
                PublishedAt = DateTime.UtcNow.AddDays(-1),
                Author = "Chris Brown",
                Source = "Learning Hub",
                Content = $"Whether you're a complete beginner or looking to expand your knowledge, this guide covers everything you need to know about {query}."
            }
        ];
    }
}
