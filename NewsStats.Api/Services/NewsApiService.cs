using System.Text;
using NewsStats.Api.Models;

namespace NewsStats.Api.Services;

public class NewsApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public NewsApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["NewsApiKey"] ?? throw new InvalidOperationException("NewsApiKey not configured.");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NewStatsApp");
    }

    public async Task<NewsApiResponse?> GetArticlesAsync(string query, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var requestUri = new StringBuilder($"https://newsapi.org/v2/everything?q={Uri.EscapeDataString(query)}");

        if (fromDate.HasValue) requestUri.Append($"&from={fromDate.Value:yyyy-MM-dd}");
        if (toDate.HasValue) requestUri.Append($"&to={toDate.Value:yyyy-MM-dd}");

        requestUri.Append($"&apiKey={_apiKey}");

        try
        {
            return await _httpClient.GetFromJsonAsync<NewsApiResponse>(requestUri.ToString());
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Error fetching articles: {e.Message}");
            return null;
        }
    }
}
